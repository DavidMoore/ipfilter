using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace IPFilter.Tests
{
    [TestFixture]
    public class SourceForgeMirrorTests
    {
        SourceForgeMirrorProvider mirror;
        Mock<SourceForgeMirrorListDownloader> downloader;
        SourceForgeMirrorParser parser;

        private const string response =
            @"<ul></ul><div style=""width: 400px;"">
	<div id=""dynimo"">
		<a href=""http://www.transact.com.au/""><img src=""http://images.sourceforge.net/mirrorimages/transact-small.png""></a>
		<p>
			Select a different mirror: 
		</p>
		<form method=""POST"" action=""/project/downloading.php"" rel=""direct"">
			<input type=""hidden"" name=""group_id"" value=""92411"">
			<input type=""hidden"" name=""filesize"" value="""">
			<input type=""hidden"" name=""filename"" value=""ipfilter.zip"">
			<input type=""hidden"" name=""abmode"" value=""""/>
			<ul class=""dynimo"">
				<li class=""current""><label><input type=""radio"" name=""use_mirror"" value=""transact"" checked=""checked"" />Transact (Canberra, Australia)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""internode"" />Internode (Adelaide, Australia)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""waix"" />Waix (Perth, Australia)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""nchc"" />National Center for High-Performance Computing (Tainan, Taiwan)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""superb-west"" />Superb Internet (Seattle, Washington)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""softlayer"" />Softlayer (Plano, TX)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""superb-east"" />Superb Internet (McLean, Virginia)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""ufpr"" />Centro de Computacao Cientifica e Software Livre (Curitiba, Brazil)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""hivelocity"" />Hivelocity (Tampa, Florida)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""internap"" />Internap Network Services (San Jose, CA)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""voxel"" />Voxel (New York, New York)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""sunet"" />Swedish University Computer Network (Sweden)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""dfn"" />German Research Network (Berlin, Germany)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""heanet"" />HEAnet (Dublin, Ireland)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""ovh"" />OVH (Paris, France)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""kent"" />University of Kent (Kent, UK)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""surfnet"" />SURFnet (Amsterdam, The Netherlands)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""switch"" />SWITCH (Lausanne, Switzerland)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""garr"" />garr.it (Bologna, Italy)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""puzzle"" />Puzzle (Bern, Switzerland)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""freefr"" />Free France (France)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""fastbull"" />Fastbull (Torino, Italy)</label></li><li><label><input type=""radio"" name=""use_mirror"" value=""Auto-select"" />Auto-select</label></li>
			</ul>
			<p>
				<input type=""submit"" name=""upd_mirror"" onclick=""init_download('http://downloads.sourceforge.net/sourceforge/emulepawcio/ipfilter.zip?use_mirror='+jQuery('input:radio[name=use_mirror]:checked').val());"" title=""Select mirror"" value=""Ok"" />
				<br /><br />
				Documentation on SourceForge.net's mirror system can be found <a style=""color:#03c;"" href=""http://p.sf.net/sourceforge/downloading"">here</a>.
			</p>
		</form>
	</div>
</div>";

        [SetUp]
        public void Setup()
        {
            parser = new SourceForgeMirrorParser();
            
            downloader = new Mock<SourceForgeMirrorListDownloader>();

            downloader.Setup(d => d.Download()).Returns(response);

            mirror = new SourceForgeMirrorProvider(parser, downloader.Object);
        }

        [Test]
        public void GetMirrors_loads_mirror_list_from_web()
        {
            mirror.GetMirrors();
            downloader.Verify(d => d.Download());
        }

        [Test]
        public void Can_extract_list_of_mirrors_from_html()
        {
            var mirrorProvider = new SourceForgeMirrorProvider();

            var mirrors = mirrorProvider.GetMirrors(response);

            Assert.IsNotNull(mirrors);

            foreach (var mirror in mirrors)
            {
                Console.WriteLine(mirror.Id + ":" + mirror.Name);
            }

            Assert.AreEqual(23, mirrors.Count());

            Assert.IsNotNull(mirrors.FirstOrDefault(mirror => mirror.Name.Equals("Transact (Canberra, Australia)",StringComparison.OrdinalIgnoreCase)));
            Assert.IsNotNull(mirrors.FirstOrDefault(mirror => mirror.Id.Equals("transact", StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void Has_provider_name()
        {
            var mirrorProvider = new SourceForgeMirrorProvider();
            Assert.AreEqual("SourceForge.net", mirrorProvider.Name);
        }

        [Test]
        public void Invokes_download_of_list()
        {
            
        }
    }
}
