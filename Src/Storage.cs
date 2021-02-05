using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace beejee_Tasker.Src
{
    //Класс с функциями хранилища данных.
    class Storage
    {
        private static Storage instance = null;
        public static Storage Instance { get {if(instance == null) instance = new Storage(); return instance; } }

        private string authToken;
        private DateTime authTokenExpire;

        public Storage()
        {
        }
        
        public void Init()
        { 
            ReadTokenData();
        }

        void ReadTokenData()
        {
            string file = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/app.data";
            XmlDocument doc = new XmlDocument(); //Штука ужасно неудобная в отличии от XDocument, но чтобы не тащить кучу зависимостей использую её.

            authToken = string.Empty;
            authTokenExpire = DateTime.Now;

            try
            {
                doc.Load(file);
            } catch(Exception)
            {               
                return;
            }
            var root = doc.DocumentElement;
            if (root == null)
                return;

            for(XmlNode node = root.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.Name == "token")
                    authToken = node.InnerText;
                else if (node.Name == "expire")
                    authTokenExpire = DateTime.Parse(node.InnerText);
            }

            if (authTokenExpire < DateTime.Now)
                authToken = string.Empty;
        }

        

        public void SetTokenData(string token)
        { //Подразумевается что токен получен перед вызовом сохранения.
            authToken = token;
            authTokenExpire = DateTime.Now + new TimeSpan(1, 0, 0, 0); // +24 часа (время жизни токена)
            string file = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/app.data";
            
            XmlDocument doc = new XmlDocument();
            var decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            
            doc.InsertBefore(decl, root);
            var auth = doc.CreateElement(string.Empty, "auth", string.Empty);
            doc.AppendChild(auth);
            
            var node = doc.CreateElement(string.Empty, "token", string.Empty);
            node.InnerText = token;
            auth.AppendChild(node);
            
            node = doc.CreateElement(string.Empty, "expire", string.Empty);
            node.InnerText = authTokenExpire.ToString();
            auth.AppendChild(node);

            doc.Save(file);
        }

        public void ResetAuthToken()
        {
            string file = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/app.data";
            if (File.Exists(file))
                File.Delete(file);

            authToken = string.Empty;
            authTokenExpire = DateTime.Now;
        }

        public string GetToken()
        {
            if (authTokenExpire > DateTime.Now)
                return authToken;
            return string.Empty;
        }
    }
}