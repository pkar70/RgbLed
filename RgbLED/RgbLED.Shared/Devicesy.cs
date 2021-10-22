using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace RgbLed
{
    public class JedenDevice
    {
        [System.Xml.Serialization.XmlAttribute()]
        public string sId { get; set; }
        [System.Xml.Serialization.XmlAttribute()]
        public string sName { get; set; }
        [System.Xml.Serialization.XmlAttribute()]
        public string sAddr { get; set; }
        [System.Xml.Serialization.XmlAttribute()]
        public bool bWhite { get; set; }
        [System.Xml.Serialization.XmlAttribute()]
        public int iTyp { get; set; } = 0; // 1: LEDBLE, 2: Tricolor
        [System.Xml.Serialization.XmlAttribute]
        public bool bEnabled { get; set; }     // enabled na liscie - tzn. pasuje jako sterowalny
        [System.Xml.Serialization.XmlIgnore]
        public bool bSelected { get; set; }    // czy ma byc wykorzystany aktualnie
        [System.Xml.Serialization.XmlIgnore]
        public string sDisplayName { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public bool bSave { get; set; }    // czy ma byc zapisany
    }

    public class Devicesy
    {
        private Collection<JedenDevice> moItemy = new Collection<JedenDevice>();
        private const string FILENAME = "devicesy.xml";

        public int Count()
        {
            return moItemy.Count;
        }

        public Collection<JedenDevice> GetList()
        {
            return moItemy;
        }

        public void Add(JedenDevice oNew)
        {
            moItemy.Add(oNew);
        }

        public void Add(string sId, string sName, string sAddr, bool bWhite, int iTyp, bool bSelected, bool bEnabled, bool bSave)
        {
            var oNew = new JedenDevice();
            oNew.sId = sId;
            oNew.sName = sName;
            oNew.sAddr = sAddr;
            oNew.bWhite = bWhite;
            oNew.iTyp = iTyp;
            oNew.bEnabled = bEnabled;
            oNew.bSelected = bSelected;
            oNew.bSave = bSave;
            Add(oNew);
        }
        // Delete
        // New
        public async Task Save(bool bAll = false)
        {
            var oFile = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFileAsync(FILENAME, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            if (oFile is null)
                return;
            var oSer = new System.Xml.Serialization.XmlSerializer(moItemy.GetType());
            var oStream = await oFile.OpenStreamForWriteAsync();
            if (bAll)
            {
                oSer.Serialize(oStream, moItemy);
            }
            else
            {
                var oTemp = new Collection<JedenDevice>();
                foreach (var oItem in moItemy)
                {
                    if (oItem.bSelected == true)
                        oTemp.Add(oItem);
                }

                oSer.Serialize(oStream, oTemp);
            }

            oStream.Dispose();   // == fclose
        }

        // Load
        public async Task<bool> Load()
        {
            // ret=false gdy nie jest wczytane

            Windows.Storage.StorageFile oObj = (Windows.Storage.StorageFile)await Windows.Storage.ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(FILENAME);
            if (oObj is null)
                return false;
            var oFile = oObj as Windows.Storage.StorageFile;
            var oSer = new System.Xml.Serialization.XmlSerializer(moItemy.GetType());
            var oStream = await oFile.OpenStreamForReadAsync();
            try
            {
                moItemy = oSer.Deserialize(oStream) as Collection<JedenDevice>;
            }
            catch
            {
                return false;
            }

            foreach (var oItem in moItemy)
                oItem.bSave = true;
            return true;
        }
    }
}