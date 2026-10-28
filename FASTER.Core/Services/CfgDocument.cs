using System.Text;

namespace FASTER.Services;

// A literal CFG reader. It never evaluates script expressions or preprocessor directives.
internal sealed class CfgDocument
{
    internal sealed record Entry(string Name, int Start, int End, int ValueStart, int ValueEnd,
        List<string> Values, List<Entry>? Children);

    private sealed record Token(string Text, int Start, int End);
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private int _position;
    public List<Entry> Entries { get; }

    public CfgDocument(string source)
    {
        _source = source;
        Tokenize();
        Entries = ReadEntries(false);
    }

    public string Raw(Entry entry) => _source[entry.Start..entry.End];

    private FormatException Error(string message, int offset) =>
        new($"{message} (line {_source[..offset].Count(c => c == '\n') + 1}).");

    private void Tokenize()
    {
        for (int i = 0; i < _source.Length;)
        {
            if (SkipTrivia(ref i)) continue;
            int start = i;
            if (_source[i] == '"') i = ReadStringEnd(i);
            else if ("{}[]=;,:".Contains(_source[i])) i++;
            else i = ReadLiteralEnd(i);
            _tokens.Add(new(_source[start..i], start, i));
        }
    }

    private bool SkipTrivia(ref int position)
    {
        if (char.IsWhiteSpace(_source[position])) { position++; return true; }
        if (_source.AsSpan(position).StartsWith("//", StringComparison.Ordinal))
        {
            while (position < _source.Length && _source[position] != '\n') position++;
            return true;
        }
        if (!_source.AsSpan(position).StartsWith("/*", StringComparison.Ordinal)) return false;
        int end = _source.IndexOf("*/", position + 2, StringComparison.Ordinal);
        if (end < 0) throw Error("Unclosed comment", position);
        position = end + 2;
        return true;
    }

    private int ReadStringEnd(int start)
    {
        int position = start + 1;
        while (position < _source.Length)
        {
            if (_source[position++] != '"') continue;
            if (position < _source.Length && _source[position] == '"') { position++; continue; }
            return position;
        }
        throw Error("Unclosed string", start);
    }

    private int ReadLiteralEnd(int position)
    {
        while (position < _source.Length && !char.IsWhiteSpace(_source[position]) &&
               !"{}[]=;,:\"".Contains(_source[position]) &&
               !_source.AsSpan(position).StartsWith("//", StringComparison.Ordinal) &&
               !_source.AsSpan(position).StartsWith("/*", StringComparison.Ordinal)) position++;
        return position;
    }

    private Token Take()
    {
        if (_position == _tokens.Count) throw Error("Unexpected end of CFG", _source.Length);
        return _tokens[_position++];
    }

    private Token Expect(string text)
    {
        var token = Take();
        if (token.Text != text) throw Error($"Expected '{text}'", token.Start);
        return token;
    }

    private List<Entry> ReadEntries(bool nested)
    {
        List<Entry> entries = [];
        while (_position < _tokens.Count && _tokens[_position].Text != "}")
        {
            var first = Take();
            if (first.Text == ";") continue;
            if (first.Text.Equals("class", StringComparison.OrdinalIgnoreCase))
            {
                var entry = ReadClass(first);
                if (entry != null) entries.Add(entry);
            }
            else entries.Add(ReadAssignment(first));
        }
        if (!nested && _position != _tokens.Count) throw Error("Unexpected closing brace", _tokens[_position].Start);
        return entries;
    }

    private Entry? ReadClass(Token first)
    {
        var name = Take();
        var next = Take();
        if (next.Text == ":") { Take(); next = Take(); }
        if (next.Text == ";") return null; // Forward declaration, no values to import.
        if (next.Text != "{") throw Error("Expected class body", next.Start);
        var children = ReadEntries(true);
        var close = Expect("}");
        var end = Expect(";");
        return new(name.Text, first.Start, end.End, next.End, close.Start, [], children);
    }

    private Entry ReadAssignment(Token first)
    {
        string key = first.Text;
        var equals = Take();
        if (equals.Text == "[") { Expect("]"); equals = Take(); key += "[]"; }
        if (equals.Text != "=") throw Error($"Expected '=' after {key}", equals.Start);
        int valueStart = _position < _tokens.Count ? _tokens[_position].Start : equals.End;
        List<string> values = [];
        int depth = 0;
        Token last;
        do
        {
            last = Take();
            if (last.Text == ";" && depth == 0) break;
            if (last.Text == "{") depth++;
            if (last.Text == "}" && depth == 0)
                throw Error("Unexpected closing brace", last.Start);
            if (last.Text == "}") depth--;
            values.Add(last.Text);
        } while (true);
        return new(key, first.Start, last.End, valueStart, last.Start, values, null);
    }

    public static string StringValue(string token)
    {
        if (token.Length < 2 || token[0] != '"' || token[^1] != '"')
            throw new FormatException("Expected a quoted string");
        return token[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal);
    }

    public static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    // Change only settings changed in the form. Keep pasted comments and custom directives.
    public static string Merge(string source, string previous, string generated)
    {
        try
        {
            var target = new CfgDocument(source);
            var before = new CfgDocument(previous);
            var after = new CfgDocument(generated);
            var plan = new MergePlan(before, after);
            var edits = plan.Build(target.Entries, before.Entries, after.Entries, source.Length);
            return ApplyEdits(source, edits);
        }
        catch (FormatException)
        {
            // A user may be midway through typing a CFG. Leave that draft intact.
            return source;
        }
    }

    private sealed record Edit(int Start, int End, string Text);

    private static string ApplyEdits(string source, List<Edit> edits)
    {
        var result = new StringBuilder(source);
        foreach (var edit in edits.OrderByDescending(e => e.Start))
            result.Remove(edit.Start, edit.End - edit.Start).Insert(edit.Start, edit.Text);
        return result.ToString();
    }

    private sealed class MergePlan(CfgDocument before, CfgDocument after)
    {
        private readonly List<Edit> _edits = [];

        public List<Edit> Build(List<Entry> targets, List<Entry> oldEntries, List<Entry> newEntries, int insertion)
        {
            MergeEntries(targets, oldEntries, newEntries, insertion);
            return _edits;
        }

        private void MergeEntries(List<Entry> targets, List<Entry> oldEntries, List<Entry> newEntries, int insertion)
        {
            foreach (var name in oldEntries.Concat(newEntries).Select(e => e.Name).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var old = oldEntries.LastOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                var next = newEntries.LastOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (old != null && next != null && before.Raw(old) == after.Raw(next)) continue;
                var matches = targets.Where(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count == 0)
                {
                    if (next != null) _edits.Add(new(insertion, insertion, "\n" + after.Raw(next) + "\n"));
                    continue;
                }
                foreach (var match in matches) MergeEntry(match, old, next);
            }
        }

        private void MergeEntry(Entry match, Entry? old, Entry? next)
        {
            if (next == null) _edits.Add(new(match.Start, match.End, ""));
            else if (match.Children != null && old?.Children != null && next.Children != null &&
                     !next.Name.Equals("Missions", StringComparison.OrdinalIgnoreCase))
                MergeEntries(match.Children, old.Children, next.Children, match.ValueEnd);
            else if (match.Children == null && next.Children == null)
                _edits.Add(new(match.ValueStart, match.ValueEnd, after._source[next.ValueStart..next.ValueEnd]));
            else _edits.Add(new(match.Start, match.End, after.Raw(next)));
        }
    }

}
