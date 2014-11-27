using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using MySql.Data.MySqlClient;

namespace MySQLHelper
{
    public static class MySQLHelper
    {
        static MySqlConnection conn;

        public static void Connect()
        {
            string myConnectionString = "server=127.0.0.1;uid=root;pwd=123456;database=peach;";

            try
            {
                Console.WriteLine("try to connect");

                conn = new MySqlConnection(myConnectionString);
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine("fail to connect");
                Console.Write(ex.Message);
            }
        }

        public static void DisConnect()
        {
            conn.Close();
        }

        static int GetMutatorID(string MutatorName)
        {
            int ret = 0;

            string Query = "select id from Mutator where name='" + MutatorName + "'";

            MySqlDataAdapter adapter = new MySqlDataAdapter(Query, conn);

            System.Data.DataTable dtable = new System.Data.DataTable();
            adapter.Fill(dtable);

            if (1 != dtable.Rows.Count)
            {
                Console.WriteLine("Does not exist Mutator which name='" + MutatorName + "' or more than one");
            }
            else
            {
                ret = int.Parse(dtable.Rows[0][0].ToString());
            }

            return ret;
        }

        static void UpdateMutationTimes(int mutatorID, uint position, uint times)
        {
            string updateCmd = "update Mutation set times = " + times
                + " where mutatorID = " + mutatorID + " and position = " + position;

            MySqlCommand cmd = new MySqlCommand(updateCmd, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Close();
        }

        static void InsertMutationTimes(int mutatorID, uint position, uint times)
        {
            string insertCmd = "insert into Mutation (mutatorID, position, times ) "
                + " values (" + mutatorID + "," + position + ", " + times + ") ";

            MySqlCommand cmd = new MySqlCommand(insertCmd, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Close();
        }

        public static void Add(string mutatorName, uint position, uint add)
        {
            int mutatorID = GetMutatorID(mutatorName);

            if (0 == mutatorID)
                return;

            // 1. Is the position exist ?

            // 1.1 exist
            //     Update the times = times + add

            // 1.2 Does not exist
            //     Insert the position and times = add

            // 1. Is the position exist ?
            uint times = GetMutationTimes(mutatorID, position);
            if (0 != times)
            {
                // 1.1
                UpdateMutationTimes(mutatorID, position, times + add);
            }
            else
            {
                // 1.2
                InsertMutationTimes(mutatorID, position, add);
            }
        }

        static uint GetMutationTimes(int mutatorID, uint position)
        {
            string Query = "select times " +
            "from Mutation " +
            "where mutatorID =" + mutatorID + " and position=" + position;

            MySqlDataAdapter adapter = new MySqlDataAdapter(Query, conn);

            DataTable dtable = new DataTable();
            adapter.Fill(dtable);

            uint ret = 0;

            if (1 == dtable.Rows.Count)
                ret = uint.Parse(dtable.Rows[0][0].ToString());

            return ret;
        }

        public static void GetMutationTimes(string MutatorName, ref List<Tuple<uint, uint>> pos_times)
        {
            string Query = "select Mutation.position, Mutation.times " +
            "from Mutation, Mutator " +
            "where Mutator.name = '" + MutatorName + "' and Mutation.mutatorID = Mutator.id";

            MySqlDataAdapter adapter = new MySqlDataAdapter(Query, conn);

            System.Data.DataTable dtable = new System.Data.DataTable();
            adapter.Fill(dtable);

            // Console.WriteLine("table size=" + dtable.Rows.Count);

            foreach (DataRow dr in dtable.Rows)
            {
                uint position = uint.Parse(dr[0].ToString());
                uint times = uint.Parse(dr[1].ToString());
                pos_times.Add(new Tuple<uint, uint>(position, times));
            }
        }
    }
}
