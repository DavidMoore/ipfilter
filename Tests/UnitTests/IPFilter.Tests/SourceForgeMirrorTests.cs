using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPFilter.Tests
{
    [TestClass]
    public class SourceForgeMirrorTests
    {
        SourceForgeMirrorProvider mirrorProvider;
        Mock<ISourceForgeMirrorListDownloader> downloader;
        SourceForgeMirrorParser parser;

        private const string TestResponseHtml =
            @"<ul id=""mirrorList"">
                        
                        <li id=""autoselect"">
                            <input type=""radio"" name=""mirror"" id=""mirror_autoselect"" value=""autoselect,Auto-select"" checked=""checked""/>
                            <label for=""mirror_autoselect"">Auto-select</label> 
                        </li>
                        
                        <li id=""hivelocity"">
                            <input type=""radio"" name=""mirror"" id=""mirror_hivelocity"" value=""hivelocity,Hivelocity"" />
                            <label for=""mirror_hivelocity"">Hivelocity</label> (Tampa, FL, US)
                        </li>
                        
                        <li id=""cdnetworks-us-2"">
                            <input type=""radio"" name=""mirror"" id=""mirror_cdnetworks-us-2"" value=""cdnetworks-us-2,CDNetworks"" />
                            <label for=""mirror_cdnetworks-us-2"">CDNetworks</label> (San Jose, CA, US)
                        </li>
                        
                        <li id=""cdnetworks-us-1"">
                            <input type=""radio"" name=""mirror"" id=""mirror_cdnetworks-us-1"" value=""cdnetworks-us-1,CDNetworks"" />
                            <label for=""mirror_cdnetworks-us-1"">CDNetworks</label> (San Jose, CA, US)
                        </li>
                        
                        <li id=""voxel"">
                            <input type=""radio"" name=""mirror"" id=""mirror_voxel"" value=""voxel,Managed Hosting and Open Source Hosting by Voxel"" />
                            <label for=""mirror_voxel"">Managed Hosting and Open Source Hosting by Voxel</label> (Brooklyn, NY, US)
                        </li>
                        
                        <li id=""superb-sea2"">
                            <input type=""radio"" name=""mirror"" id=""mirror_superb-sea2"" value=""superb-sea2,Superb Internet"" />
                            <label for=""mirror_superb-sea2"">Superb Internet</label> (Tukwila, WA, US)
                        </li>
                        
                        <li id=""iweb"">
                            <input type=""radio"" name=""mirror"" id=""mirror_iweb"" value=""iweb,iWeb"" />
                            <label for=""mirror_iweb"">iWeb</label> (Montreal, QC, CA)
                        </li>
                        
                        <li id=""heanet"">
                            <input type=""radio"" name=""mirror"" id=""mirror_heanet"" value=""heanet,HEAnet"" />
                            <label for=""mirror_heanet"">HEAnet</label> (Dublin, Ireland, IE)
                        </li>
                        
                        <li id=""jaist"">
                            <input type=""radio"" name=""mirror"" id=""mirror_jaist"" value=""jaist,Japan Advanced Institute of Science and Technology"" />
                            <label for=""mirror_jaist"">Japan Advanced Institute of Science and Technology</label> (Japan, JP)
                        </li>
                        
                        <li id=""sunet"">
                            <input type=""radio"" name=""mirror"" id=""mirror_sunet"" value=""sunet,Swedish University Computer Network"" />
                            <label for=""mirror_sunet"">Swedish University Computer Network</label> (Uppsala, Sweden, SE)
                        </li>
                        
                        <li id=""ufpr"">
                            <input type=""radio"" name=""mirror"" id=""mirror_ufpr"" value=""ufpr,Centro de Computacao Cientifica e Software Livre"" />
                            <label for=""mirror_ufpr"">Centro de Computacao Cientifica e Software Livre</label> (Brazil, BR)
                        </li>
                        
                        <li id=""kent"">
                            <input type=""radio"" name=""mirror"" id=""mirror_kent"" value=""kent,University of Kent"" />
                            <label for=""mirror_kent"">University of Kent</label> (Kent, United Kingdom, GB)
                        </li>
                        
                        <li id=""freefr"">
                            <input type=""radio"" name=""mirror"" id=""mirror_freefr"" value=""freefr,Free France"" />
                            <label for=""mirror_freefr"">Free France</label> (Paris, France, FR)
                        </li>
                        
                        <li id=""ovh"">
                            <input type=""radio"" name=""mirror"" id=""mirror_ovh"" value=""ovh,OVH"" />
                            <label for=""mirror_ovh"">OVH</label> (Paris, France, FR)
                        </li>
                        
                        <li id=""mesh"">
                            <input type=""radio"" name=""mirror"" id=""mirror_mesh"" value=""mesh,Mesh Solutions"" />
                            <label for=""mirror_mesh"">Mesh Solutions</label> (Dusseldorf, Germany, DE)
                        </li>
                        
                        <li id=""cdnetworks-kr-2"">
                            <input type=""radio"" name=""mirror"" id=""mirror_cdnetworks-kr-2"" value=""cdnetworks-kr-2,CDNetworks"" />
                            <label for=""mirror_cdnetworks-kr-2"">CDNetworks</label> (Seoul, Korea, Republic of, KR)
                        </li>
                        
                        <li id=""cdnetworks-kr-1"">
                            <input type=""radio"" name=""mirror"" id=""mirror_cdnetworks-kr-1"" value=""cdnetworks-kr-1,CDNetworks"" />
                            <label for=""mirror_cdnetworks-kr-1"">CDNetworks</label> (Seoul, Korea, Republic of, KR)
                        </li>
                        
                        <li id=""surfnet"">
                            <input type=""radio"" name=""mirror"" id=""mirror_surfnet"" value=""surfnet,SWITCH"" />
                            <label for=""mirror_surfnet"">SWITCH</label> (Zurich, Switzerland, CH)
                        </li>
                        
                        <li id=""switch"">
                            <input type=""radio"" name=""mirror"" id=""mirror_switch"" value=""switch,SWITCH"" />
                            <label for=""mirror_switch"">SWITCH</label> (Zurich, Switzerland, CH)
                        </li>
                        
                        <li id=""ignum"">
                            <input type=""radio"" name=""mirror"" id=""mirror_ignum"" value=""ignum,Ignum"" />
                            <label for=""mirror_ignum"">Ignum</label> (Prague, Czech Republic, CZ)
                        </li>
                        
                        <li id=""ncu"">
                            <input type=""radio"" name=""mirror"" id=""mirror_ncu"" value=""ncu,National Central University"" />
                            <label for=""mirror_ncu"">National Central University</label> (Taoyuan, Taiwan, TW)
                        </li>
                        
                        <li id=""nchc"">
                            <input type=""radio"" name=""mirror"" id=""mirror_nchc"" value=""nchc,National Center for High-Performance Computing"" />
                            <label for=""mirror_nchc"">National Center for High-Performance Computing</label> (Hsinchu, Taiwan, TW)
                        </li>
                        
                        <li id=""transact"">
                            <input type=""radio"" name=""mirror"" id=""mirror_transact"" value=""transact,Transact"" />
                            <label for=""mirror_transact"">Transact</label> (Canberra, Australia, AU)
                        </li>
                        
                        <li id=""internode"">
                            <input type=""radio"" name=""mirror"" id=""mirror_internode"" value=""internode,Internode"" />
                            <label for=""mirror_internode"">Internode</label> (Adelaide, Australia, AU)
                        </li>
                        
                        <li id=""waix"">
                            <input type=""radio"" name=""mirror"" id=""mirror_waix"" value=""waix,Waix"" />
                            <label for=""mirror_waix"">Waix</label> (Perth, Australia, AU)
                        </li>
                        
                    </ul>";

        [TestInitialize]
        public void Setup()
        {
            parser = new SourceForgeMirrorParser();
            
            downloader = new Mock<ISourceForgeMirrorListDownloader>();

            downloader.Setup(d => d.Download()).Returns(TestResponseHtml);

            mirrorProvider = new SourceForgeMirrorProvider(parser, downloader.Object);
        }

        [TestMethod]
        public void GetMirrors_loads_mirror_list_from_web()
        {
            mirrorProvider.GetMirrors();
            downloader.Verify(d => d.Download());
        }

        [TestMethod]
        public void Can_extract_list_of_mirrors_from_html()
        {
            var provider = new SourceForgeMirrorProvider();

            var mirrors = provider.GetMirrors(TestResponseHtml);

            Assert.IsNotNull(mirrors);

            foreach (var mirror in mirrors)
            {
                Console.WriteLine(mirror.Id + ":" + mirror.Name);
            }

            Assert.AreEqual(25, mirrors.Count());

            Assert.IsNotNull(mirrors.FirstOrDefault(mirror => mirror.Name.Equals("Transact (Canberra, Australia)",StringComparison.OrdinalIgnoreCase)));
            Assert.IsNotNull(mirrors.FirstOrDefault(mirror => mirror.Id.Equals("transact", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void Has_provider_name()
        {
            var provider = new SourceForgeMirrorProvider();
            Assert.AreEqual("SourceForge.net", provider.Name);
        }
    }
}
