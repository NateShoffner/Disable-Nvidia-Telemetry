#region

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

#endregion

namespace DisableNvidiaTelemetry
{
    internal class PortableSettingsProvider : SettingsProvider
    {
        private const string APPLICATION_NAME = "DisableNvidiaTelemetry";
        private const string CONFIG_NAME = "Settings.config";

        // Define some static strings later used in our XML creation
        // XML Root node
        private const string XMLROOT = "configuration";

        // Configuration declaration node
        private const string CONFIGNODE = "configSections";

        // Configuration section group declaration node
        private const string GROUPNODE = "sectionGroup";

        // User section node
        private const string USERNODE = "userSettings";

        // Application Specific Node
        private const string APPNODE = APPLICATION_NAME + ".Properties.Settings";

        private XmlDocument xmlDoc;
        public override string Name => "PortableSettingsProvider";

        // Override the ApplicationName property, returning the solution name.  No need to set anything, we just need to
        // retrieve information, though the set method still needs to be defined.
        public override string ApplicationName
        {
            get { return APPLICATION_NAME; }
            set { }
        }

        private XmlDocument XMLConfig
        {
            get
            {
                // Check if we already have accessed the XML config file. If the xmlDoc object is empty, we have not.
                if (xmlDoc == null)
                {
                    xmlDoc = new XmlDocument();

                    // If we have not loaded the config, try reading the file from disk.
                    try
                    {
                        xmlDoc.Load(Path.Combine(GetAppPath(), GetSettingsFilename()));
                    }

                    // If the file does not exist on disk, catch the exception then create the XML template for the file.
                    catch (Exception)
                    {
                        // XML Declaration
                        // <?xml version="1.0" encoding="utf-8"?>
                        var dec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                        xmlDoc.AppendChild(dec);

                        // Create root node and append to the document
                        // <configuration>
                        var rootNode = xmlDoc.CreateElement(XMLROOT);
                        xmlDoc.AppendChild(rootNode);

                        // Create Configuration Sections node and add as the first node under the root
                        // <configSections>
                        var configNode = xmlDoc.CreateElement(CONFIGNODE);
                        xmlDoc.DocumentElement.PrependChild(configNode);

                        // Create the user settings section group declaration and append to the config node above
                        // <sectionGroup name="userSettings"...>
                        var groupNode = xmlDoc.CreateElement(GROUPNODE);
                        groupNode.SetAttribute("name", USERNODE);
                        groupNode.SetAttribute("type", "System.Configuration.UserSettingsGroup");
                        configNode.AppendChild(groupNode);

                        // Create the Application section declaration and append to the groupNode above
                        // <section name="AppName.Properties.Settings"...>
                        var newSection = xmlDoc.CreateElement("section");
                        newSection.SetAttribute("name", APPNODE);
                        newSection.SetAttribute("type", "System.Configuration.ClientSettingsSection");
                        groupNode.AppendChild(newSection);

                        // Create the userSettings node and append to the root node
                        // <userSettings>
                        var userNode = xmlDoc.CreateElement(USERNODE);
                        xmlDoc.DocumentElement.AppendChild(userNode);

                        // Create the Application settings node and append to the userNode above
                        // <AppName.Properties.Settings>
                        var appNode = xmlDoc.CreateElement(APPNODE);
                        userNode.AppendChild(appNode);
                    }
                }
                return xmlDoc;
            }
        }

        // Override the Initialize method
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        // Simply returns the name of the settings file, which is the solution name plus ".config"
        public virtual string GetSettingsFilename()
        {
            return CONFIG_NAME;
        }

        // Gets current executable path in order to determine where to read and write the config file
        public virtual string GetAppPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        // Retrieve settings from the configuration file
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sContext, SettingsPropertyCollection settingsColl)
        {
            // Create a collection of values to return
            var retValues = new SettingsPropertyValueCollection();

            // Create a temporary SettingsPropertyValue to reuse

            // Loop through the list of settings that the application has requested and add them
            // to our collection of return values.
            foreach (SettingsProperty sProp in settingsColl)
            {
                var setVal = new SettingsPropertyValue(sProp) { IsDirty = false, SerializedValue = GetSetting(sProp) };
                retValues.Add(setVal);
            }
            return retValues;
        }

        // Save any of the applications settings that have changed (flagged as "dirty")
        public override void SetPropertyValues(SettingsContext sContext, SettingsPropertyValueCollection settingsColl)
        {
            // Set the values in XML
            foreach (SettingsPropertyValue spVal in settingsColl)
            {
                SetSetting(spVal);
            }

            // Write the XML file to disk
            try
            {
                XMLConfig.Save(Path.Combine(GetAppPath(), GetSettingsFilename()));
            }
            catch
            {
                //unhandled
            }
        }

        // Retrieve values from the configuration file, or if the setting does not exist in the file,
        // retrieve the value from the application's default configuration
        private object GetSetting(SettingsProperty setProp)
        {
            object retVal;

            try
            {
                if (setProp.SerializeAs.ToString() == "String")
                    return XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild.InnerText;

                var xmlData = XMLConfig.SelectSingleNode(string.Format("//setting[@name='{0}']", setProp.Name)).FirstChild.InnerXml;
                return string.Format(@"{0}", xmlData);
            }

            catch (Exception)
            {
                if (setProp.DefaultValue != null)
                    if (setProp.SerializeAs.ToString() == "String")
                    {
                        retVal = setProp.DefaultValue.ToString();
                    }
                    else
                    {
                        var settingType = setProp.PropertyType.ToString();
                        var xmlData = setProp.DefaultValue.ToString();
                        var xs = new XmlSerializer(typeof(string[]));
                        var data = (string[])xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));

                        switch (settingType)
                        {
                            case "System.Collections.Specialized.StringCollection":
                                var sc = new StringCollection();
                                sc.AddRange(data);
                                return sc;

                            default:
                                return "";
                        }
                    }
                else
                    retVal = "";
            }
            return retVal;
        }

        private void SetSetting(SettingsPropertyValue setProp)
        {
            // Define the XML path under which we want to write our settings if they do not already exist
            XmlNode settingNode;

            try
            {
                // Search for the specific settings node we want to update.
                // If it exists, return its first child node, (the <value>data here</value> node)
                settingNode = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild;
            }
            catch (Exception)
            {
                settingNode = null;
            }

            // If we have a pointer to an actual XML node, update the value stored there
            if (settingNode != null)
            {
                if (setProp.Property.SerializeAs.ToString() == "String")
                    settingNode.InnerText = setProp.SerializedValue.ToString();
                else
                    settingNode.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");
            }
            else
            {
                // If the value did not already exist in this settings file, create a new entry for this setting

                // Search for the application settings node (<Appname.Properties.Settings>) and store it.
                var tmpNode = XMLConfig.SelectSingleNode("//" + APPNODE);

                // Create a new settings node and assign its name as well as how it will be serialized
                var newSetting = xmlDoc.CreateElement("setting");
                newSetting.SetAttribute("name", setProp.Name);

                newSetting.SetAttribute("serializeAs", setProp.Property.SerializeAs.ToString() == "String" ? "String" : "Xml");

                // Append this node to the application settings node (<Appname.Properties.Settings>)
                tmpNode.AppendChild(newSetting);

                // Create an element under our named settings node, and assign it the value we are trying to save
                var valueElement = xmlDoc.CreateElement("value");
                if (setProp.Property.SerializeAs.ToString() == "String")
                    valueElement.InnerText = setProp.SerializedValue.ToString();
                else
                    valueElement.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");

                //Append this new element under the setting node we created above
                newSetting.AppendChild(valueElement);
            }
        }
    }
}