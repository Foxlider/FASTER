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
            Assert.AreEqual(" ", _converter.Convert(false, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
            Assert.AreEqual("Invalid regular expression...", _converter.Convert(true, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
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

            Assert.AreEqual(new SolidColorBrush(Color.FromRgb(90, 29, 29)).Color, c1.Color);
            Assert.AreEqual(new SolidColorBrush().Color, c2.Color);
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

            Assert.AreEqual(new SolidColorBrush(Color.FromRgb(190, 17, 0)).Color, c1.Color);
            Assert.AreEqual(new SolidColorBrush().Color, c2.Color); 
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
            Assert.AreEqual(System.Windows.Visibility.Visible, _converter.Convert(false, typeof(System.Windows.Visibility), null, System.Globalization.CultureInfo.InvariantCulture));
            Assert.AreEqual(System.Windows.Visibility.Collapsed, _converter.Convert(true, typeof(System.Windows.Visibility), null, System.Globalization.CultureInfo.InvariantCulture));
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
            Assert.AreEqual($"   1{separator}00  B", _converter.Convert((long)1, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
            Assert.AreEqual($"   1{separator}00 KB", _converter.Convert((long)1024, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
            Assert.AreEqual("0 B", _converter.Convert("FAIL", typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
        }

        [Test()]
        public void ConvertBackTest()
        {
            Assert.IsNull(_converter.ConvertBack(1, typeof(string), null, null));
        }
    }
}