using NUnit.Framework;

using System;
using System.Windows.Media;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ProfileModsFilterIsInvalidTextConverterTests
    {
        readonly ProfileModsFilterIsInvalidTextConverter _converter = new();

        [Test()]
        public void ConvertTest()
        {
            Assert.That(_converter.Convert(false, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(" "));
            Assert.That(_converter.Convert(true, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo("Invalid regular expression..."));
        }

        [Test()]
        public void ConvertBackTest()
        {
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(null, null, null, null));
        }
    }

    [TestFixture()]
    public class ProfileModsFilterIsInvalidBackgroundColorConverterTests
    {
        readonly ProfileModsFilterIsInvalidBackgroundColorConverter _converter = new();

        [Test()]
        public void ConvertTest()
        {
            SolidColorBrush c1 = _converter.Convert(true, typeof(SolidColorBrush), null, System.Globalization.CultureInfo.InvariantCulture) as SolidColorBrush;
            SolidColorBrush c2 = _converter.Convert(false, typeof(SolidColorBrush), null, System.Globalization.CultureInfo.InvariantCulture) as SolidColorBrush;

            Assert.That(c1.Color, Is.EqualTo(new SolidColorBrush(Color.FromRgb(90, 29, 29)).Color));
            Assert.That(c2.Color, Is.EqualTo(new SolidColorBrush().Color));
        }

        [Test()]
        public void ConvertBackTest()
        {
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(null, null, null, null));
        }
    }

    [TestFixture()]
    public class ProfileModsFilterIsInvalidBorderColorConverterTests
    {
        readonly ProfileModsFilterIsInvalidBorderColorConverter _converter = new();
        [Test()]
        public void ConvertTest()
        {
            SolidColorBrush c1 = _converter.Convert(true, typeof(SolidColorBrush), null, System.Globalization.CultureInfo.InvariantCulture) as SolidColorBrush;
            SolidColorBrush c2 = _converter.Convert(false, typeof(SolidColorBrush), null, System.Globalization.CultureInfo.InvariantCulture) as SolidColorBrush;

            Assert.That(c1.Color, Is.EqualTo(new SolidColorBrush(Color.FromRgb(190, 17, 0)).Color));
            Assert.That(c2.Color, Is.EqualTo(new SolidColorBrush().Color)); 
        }

        [Test()]
        public void ConvertBackTest()
        {
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(null, null, null, null));
        }
    }

    [TestFixture()]
    public class NotBooleanToVisibilityConverterTests
    {
        readonly NotBooleanToVisibilityConverter _converter = new();
        [Test()]
        public void ConvertTest()
        {
            Assert.That(_converter.Convert(false, typeof(System.Windows.Visibility), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(System.Windows.Visibility.Visible));
            Assert.That(_converter.Convert(true, typeof(System.Windows.Visibility), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(System.Windows.Visibility.Collapsed));
        }

        [Test()]
        public void ConvertBackTest()
        {
            Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(null, null, null, null));
        }
    }

    [TestFixture()]
    public class FolderSizeConverterTests
    {
        readonly FolderSizeConverter _converter = new();
        [Test()]
        public void ConvertTest()
        {
            var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.That(_converter.Convert((long)1, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo($"   1{separator}00  B"));
            Assert.That(_converter.Convert((long)1024, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo($"   1{separator}00 KB"));
            Assert.That(_converter.Convert("FAIL", typeof(string), null, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo("0 B"));
        }

        [Test()]
        public void ConvertBackTest()
        {
            Assert.That(_converter.ConvertBack(1, typeof(string), null, null), Is.Null);
        }
    }
}