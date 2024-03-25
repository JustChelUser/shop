using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Reflection.PortableExecutable;
namespace shop_;

internal class Program
{
    static void Main(string[] args)
    {
        string connStr = "server=localhost;user=root;database=shop;password=MySQLServer477";
        MySqlConnection conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            //Считывание номеров заказов
            string? stringOrders = Console.ReadLine();
            string[] Orders = stringOrders.Split(',');
            for (int i = 0; i < Orders.Length; i++)
            {
                if (!int.TryParse(Orders[i], out _))
                {
                    Orders[i] = "null";
                }
            }
            //Все некорректные номера заменены на null чтобы избежать проблем с запросом
            String checkedOrders = String.Join(",", Orders);
            //Получение не основных стеллажей для товаров из заказов
            MySqlCommand cmd2 = new MySqlCommand("SELECT Rack.name as \"Rack.name\",Good.id as \"good_id\" FROM Rack  \r\ninner join Goods_Racks on Rack.id = Goods_Racks.Rack_id  \r\ninner join Good on Goods_Racks.Good_id = Good.id \r\ninner join Goods_orders on Goods_orders.Good_id = Good.id and Goods_orders.Order_id in ("+checkedOrders+") \r\ninner join Type_of_goods_Racks on Rack.id = Type_of_goods_Racks.Rack_id and main_rack = 0 and Type_of_goods_Racks.Type_of_good_id = Good.Type_of_good_id;", conn);
            MySqlDataReader rdr2 = cmd2.ExecuteReader();
            List<string> rackList = new List<string>();
            List<string> goodIdList = new List<string>();
            while (rdr2.Read())
            {
                rackList.Add(rdr2[0].ToString());
                goodIdList.Add(rdr2[1].ToString());
            }
            rdr2.Close();
            //Получение данных о стеллажах и товарах которые участвуют в заказах
            MySqlCommand cmd = new MySqlCommand("SELECT Rack.name as \"Rack.name\",Good.name as \"Good.name\",Order_id as \"Order_id\",quantity,Good.id as \"good_id\", Rack.id as \"rack_id\"  FROM Rack    \r\ninner join Goods_Racks on Rack.id = Goods_Racks.Rack_id  \r\ninner join Good on Goods_Racks.Good_id = Good.id \r\ninner join Goods_orders on Goods_orders.Good_id = Good.id and Goods_orders.Order_id in ("+checkedOrders+")\r\ninner join Type_of_goods_Racks on Rack.id = Type_of_goods_Racks.Rack_id and main_rack = 1 and Type_of_goods_Racks.Type_of_good_id = Good.Type_of_good_id\r\nORDER BY\r\n    Rack.name,\r\n    CASE\r\n        WHEN Rack.name = 'A' THEN Order_id\r\n        ELSE Rack.name\r\n    END;", conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            //переменная для хранения id стеллажа. Нужна для того чтобы выводить название стеллажа только один раз
            int idOfLastDisplayedRack = 0;
            checkedOrders.Replace("null","");
            Console.WriteLine("Страница сборки заказов " + checkedOrders.ToString());
            //логический флаг для того чтобы вывести надпись доп стеллаж 1 раз
            bool rackDisplayed = false;
            while (rdr.Read())
            {
                if (idOfLastDisplayedRack == 0 || (idOfLastDisplayedRack != 0 && idOfLastDisplayedRack != (int)rdr[5]))
                {
                    Console.WriteLine("===Стеллаж " + rdr[0]);
                    idOfLastDisplayedRack = (int)rdr[5];
                }
                Console.WriteLine(rdr[1] + " (id=" + rdr[4] + ")");
                Console.WriteLine("заказ "+rdr[2]+"," + rdr[3] +" шт");
                //поиск id текущего товара в списке товаров у которых есть не основные стеллажи
                for (int i = 0; i < goodIdList.Count; i++)
                {
                    if (goodIdList[i].ToString() == rdr[4].ToString() && rackDisplayed == false)
                    {
                        Console.Write("доп стеллаж: " + rackList[i]);
                        rackDisplayed = true;
                    }
                    else if (goodIdList[i].ToString() == rdr[4].ToString() && rackDisplayed == true)
                    {
                        Console.WriteLine("," + rackList[i]);
                    }
                }
                Console.WriteLine("\n");
                rackDisplayed = false;
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        conn.Close();
    }

}
