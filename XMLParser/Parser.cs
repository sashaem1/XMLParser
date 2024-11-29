using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XMLParser
{
    public class Parser
    {
        [XmlRoot("product")]
        public class Product
        {
            [XmlElement("quantity")]
            public int Quantity { get; set; }

            [XmlElement("name")]
            public string Name { get; set; }

            [XmlElement("price")]
            public float Price { get; set; }
        }

        [XmlRoot("user")]
        public class User
        {
            [XmlElement("fio")]
            public string FIO { get; set; }

            [XmlElement("email")]
            public string Email { get; set; }
        }


        public class Order
        {
            [XmlElement("no")]
            public int No { get; set; }

            [XmlElement("reg_date")]
            public string RegDate { get; set; }

            [XmlElement("sum")]
            public float Sum { get; set; }


            [XmlElement("product")]
            public List<Product> Products { get; set; } = new List<Product>();

            [XmlElement("user")]
            public User User { get; set; }
        }

        [XmlRoot("orders")]
        public class Orders
        {
            [XmlElement("order")]
            public Order[] OrdersList { get; set; }
        }


        static public List<Order> ParseXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Orders));
            using (FileStream fs = File.OpenRead(filePath))
            {
                Orders orders = (Orders)serializer.Deserialize(fs);
                return orders.OrdersList.ToList();
            }
        }
    }
}
