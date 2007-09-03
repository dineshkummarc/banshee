using System;
using System.Collections.Generic;


namespace Banshee.AudioProfiles
{
	class XmlProfileConfiguration : ProfileConfiguration
	{
        public XmlProfileConfiguration(Profile profile, string gconfRoot, string id)
            : base(profile, id)
        {
            //client = new GConf.Client();
            //this.gconf_root = gconfRoot;
        }
        
        protected override void Load()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Save()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
