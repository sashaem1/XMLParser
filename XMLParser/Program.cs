using System;
using System.Xml;
using static XMLParser.Parser;
using System.Xml.Serialization;
using System.IO;
using Npgsql;
using System.Globalization;
using static XMLParser.DBinteraction;

namespace XMLParser
{
    public class Program
    {
        static void Main(string[] args)
        {
            string XMLPath = "input.xml";
            List<Order> orders = ParseXml(XMLPath);

            string connectionString = "Host=localhost; Database=ParseXML; Username=postgres; Password=root";
            var dbConnection = new PostgresConnection(connectionString);
            var orderService = new OrderService(dbConnection);

            orderService.EnterDataToDB(orders);
        }
    }
}