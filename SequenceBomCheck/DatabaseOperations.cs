using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;
using System.Windows.Controls;

namespace SequenceBomCheck
{
    class DatabaseOperations
    {

        #region Connection setup & Enviroment check
        /// <summary>
        /// enviroment constant setting and methods to verify 
        /// </summary>
        private const String SERVER = "localhost";
        private const String PORT = "5432";
        private const String USER = "PRODUCTION";
        private const String PASSWORD = "eurobag";
        private const String DATABASE = "PROD_TRACING";
        private NpgsqlConnection Connection = null;

        public class PartinBom
        {
            // to represent PN + its description in the list 
            public string PartNumber { get; set; }
            public string Name { get; set; }
            public string Mode { get; set; }
        }

        private void DatabaseInit()
        {
            //create connection object
            Connection = new NpgsqlConnection(
                "Server=" + SERVER + ";" +
                "Port=" + PORT + ";" +
                "User Id=" + USER + ";" +
                "Password=" + PASSWORD + ";" +
                "Database=" + DATABASE + ";" +
                "Pooling= false" + ";" +
                "ApplicationName=SequenceContent_MMA" + ";"
                );
        }
        private void showError(NpgsqlException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void openConnection()
        {
            this.DatabaseInit();
            // this.Connection.Dispose();
            try
            {
                Connection.Open();
            }
            catch (NpgsqlException ex)
            {
                showError(ex);
            }
        }

        public void closeConnection()
        {
            //this.DatabaseInit();
            try
            {
                Connection.Close();

            }
            catch (NpgsqlException ex)
            {
                showError(ex);
            }
        }

        public bool CheckConnection()
        {
            bool result = false;
            this.openConnection();
            if (Connection.State == System.Data.ConnectionState.Open)
            {
                result = true;
            }
            this.closeConnection();
            return result;
        }

        public string getATraQLineID()
        {

            this.openConnection();
            NpgsqlCommand getLineIdQuery = new NpgsqlCommand("SELECT stocpf_line from t_stocpf limit 1", Connection);
            NpgsqlDataReader dataFromT_stocpf = getLineIdQuery.ExecuteReader();
            string ATraQLineId;

            if (dataFromT_stocpf.HasRows & dataFromT_stocpf.Read())
                ATraQLineId = dataFromT_stocpf[0].ToString();
            else
                ATraQLineId = "xxxx";

            dataFromT_stocpf.Close();
            this.closeConnection();
            return ATraQLineId;

        }
        public string getMode()
        {

            this.openConnection();
            NpgsqlCommand getLineIdQuery = new NpgsqlCommand("select contex_value from t_contex where contex_nom = '1000' and contex_param = 'SEQMODE' ", Connection);
            NpgsqlDataReader dataFromT_stocpf = getLineIdQuery.ExecuteReader();
            string result;

            if (dataFromT_stocpf.HasRows & dataFromT_stocpf.Read())
                result = dataFromT_stocpf[0].ToString();
            else
                result = "not found";

            dataFromT_stocpf.Close();
            this.closeConnection();
            return result;

        }
        public string getActiveAmoteqLine()
        {

            this.openConnection();
            NpgsqlCommand getLineIdQuery = new NpgsqlCommand("select contex_value from t_contex where contex_nom = '1000-10' and contex_param = 'AMOTICLINE'", Connection);
            NpgsqlDataReader dataFromT_stocpf = getLineIdQuery.ExecuteReader();
            string result;
            if (getMode() == "ON_SEQUENCE")
            {
                if (dataFromT_stocpf.HasRows & dataFromT_stocpf.Read())
                    result = dataFromT_stocpf[0].ToString();
                else
                    result = "not found";
            }
            else
            {
                result = null;
            }

            dataFromT_stocpf.Close();
            this.closeConnection();
            return result;

        }

        #endregion // 

        #region DATA OPERATIONS
        ///<summary>
        /// Operations on the database to get the data
        /// This class should never change data in db, only read them 
        ///</summary>
        ///

        public List<string> getSequnecesRacks(string AmoteqLine, string StatusList)
        {
            List<string> result = new List<string>();
            this.openConnection();

            string querytext = "select distinct T0.trace_order_value from t_trace_order T0 " +
                "join t_trace_order T1 on T0.id_trace_order = T1.id_trace_order and T0.et_order_carord = '11' and T1.et_order_carord = '23' " +
                "join t_trace_order T2 on T0.id_trace_order = T2.id_trace_order and T2.et_order_carord = '-2' " +
                "where T1.trace_order_value in " + StatusList +
                " and T2.trace_order_value = '" + AmoteqLine + "'";

            NpgsqlCommand query = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader dataFromTable = query.ExecuteReader();


            if (dataFromTable.HasRows)
            {
                while (dataFromTable.Read())
                {
                    result.Add(dataFromTable[0].ToString());
                }
                dataFromTable.Close();
            }
            dataFromTable.Close();
            this.closeConnection();
            return result;

        }
        public List<string> getRackPositions(string AmoteqLine, string StatusList, string RackNo)
        {
            List<string> result = new List<string>();
            this.openConnection();

            string querytext = "select distinct cast(T3.trace_order_value AS INTEGER) as result from t_trace_order T0 " +
                "join t_trace_order T1 on T0.id_trace_order = T1.id_trace_order and T0.et_order_carord = '11' and T1.et_order_carord = '23' " +
                "join t_trace_order T2 on T0.id_trace_order = T2.id_trace_order and T2.et_order_carord = '-2' " +
                "join t_trace_order T3 on T0.id_trace_order = T3.id_trace_order and T3.et_order_carord = '12' " +
                "where T1.trace_order_value in " + StatusList +
                "and T2.trace_order_value = '" + AmoteqLine +
                "' and T0.trace_order_value = '" + RackNo + "' " +
                "order by result";


            //MessageBox.Show(querytext);

            NpgsqlCommand getData = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader dataFromTable = getData.ExecuteReader();



            if (dataFromTable.HasRows)
            {
                while (dataFromTable.Read())
                {
                    result.Add(dataFromTable[0].ToString());
                }
                dataFromTable.Close();
            }
            this.closeConnection();
            return result;
        }

        public List<string> getSequenceRoot(string AmoteqLine, string RackNo, string PosInRack)
        {
            List<string> result = new List<string>();
            this.openConnection();

            string querytext = "select distinct T4.trace_order_value from t_trace_order T0 " +
                "join t_trace_order T1 on T0.id_trace_order = T1.id_trace_order and T0.et_order_carord = '11' and T1.et_order_carord = '23' " +
                "join t_trace_order T2 on T0.id_trace_order = T2.id_trace_order and T2.et_order_carord = '-2' " +
                "join t_trace_order T3 on T0.id_trace_order = T3.id_trace_order and T3.et_order_carord = '12' " +
                "join t_trace_order T4 on T0.id_trace_order = T4.id_trace_order and T4.et_order_carord = '-18' " +
                "where T2.trace_order_value = '" + AmoteqLine +
                "' and T0.trace_order_value = '" + RackNo + "' " +
                "and T3.trace_order_value = '" + PosInRack + "' " +
                "order by T4.trace_order_value";


            //MessageBox.Show(querytext);

            NpgsqlCommand getData = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader dataFromTable = getData.ExecuteReader();



            if (dataFromTable.HasRows)
            {
                while (dataFromTable.Read())
                {
                    result.Add(dataFromTable[0].ToString());
                }
                dataFromTable.Close();
            }
            this.closeConnection();
            return result;
        }

        public List<string> getFamilies(string line)
        {
            List<string> result = new List<string>();
            this.openConnection();
            string querytext = "select distinct TN.et_refakf_se from t_simo TS " +
                "join t_nomen TN on TS.et_nomen = TN.id_nomen and TN.nomen_fin_validite is null and TS.simo_fin_validite is null and TN.et_refakf_comp is null " +
                "join t_modeop TM on TS.et_modeop = TM.id_modeop and TM.modeop_fin_validite is null " +
                "join u_prod UP on UP.id_prod = TM.et_prod_poste and UP.prod_fin_validite is null " +
                "where UP.et_prod_pere = '" + line + "' " +
                "order by TN.et_refakf_se asc";
            //MessageBox.Show(querytext);

            NpgsqlCommand getData = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader dataFromTable = getData.ExecuteReader();

            if (dataFromTable.HasRows)
            {
                while (dataFromTable.Read())
                {
                    result.Add(dataFromTable[0].ToString());
                }
                dataFromTable.Close();
            }
            this.closeConnection();
            return result;
        }

        public List<PartinBom> getBillOfMaterial(string FamName)
        {
            List<PartinBom> result = new List<PartinBom>();
            this.openConnection();
            string querytext = "select TR.id_refcmp, TR.refcmp_nom, TT.typcomp_libelle from t_nomen TN " +
                "join t_refcmp TR on TN.et_refakf_comp = TR.id_refcmp and TN.et_refakf_se = 'MSLV5_LX_MPHL_HOD' and TN.nomen_fin_validite is null " +
                "join t_typcomp TT on TT.id_typcomp = TN.et_typcomp " +
                "order by TR.id_refcmp asc";

            NpgsqlCommand getdata = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader readdata = getdata.ExecuteReader();

            if (readdata.HasRows)
            {
                while (readdata.Read())
                {
                    PartinBom tmp = new PartinBom();
                    tmp.PartNumber = readdata[0].ToString();
                    tmp.Name = readdata[1].ToString();
                    tmp.Mode = readdata[2].ToString();
                    //MessageBox.Show(tmp.PartNumber + " " + tmp.Name); 
                    result.Add(tmp);
                }
                readdata.Close();
            }
            this.closeConnection();
            return result;
        }

        public List<PartinBom> getSeqBillOfMaterial(string id_trace_order)
        {
            List<PartinBom> result = new List<PartinBom>();
            this.openConnection();
            string querytext = "select TT.trace_order_value, TR.refcmp_nom from t_trace_order TT join t_refcmp TR on TT.trace_order_value = TR.id_refcmp and TT.et_order_carord = '29' " +
                " and TT.id_trace_order = '" + id_trace_order + "' order by TT.trace_order_value asc";

            NpgsqlCommand getdata = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader readdata = getdata.ExecuteReader();

            if (readdata.HasRows)
            {
                while (readdata.Read())
                {
                    PartinBom tmp = new PartinBom();
                    tmp.PartNumber = readdata[0].ToString();
                    tmp.Name = readdata[1].ToString();
                    //MessageBox.Show(tmp.PartNumber + " " + tmp.Name); 
                    result.Add(tmp);
                }
                readdata.Close();
            }
            this.closeConnection();
            return result;

        }

        public List<string> GetAmoteqLine(string line)
        {
            List<string> result = new List<string>();
            this.openConnection();
            string querytext = "select inival_value from t_inival where et_prod = '" + line + "' and et_tyintr = 'SEQUENCE' and et_key = 'AMOTIQ_LINE' and inival_used = '1'";

            //MessageBox.Show(querytext);

            NpgsqlCommand getData = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader dataFromTable = getData.ExecuteReader();



            if (dataFromTable.HasRows)
            {
                while (dataFromTable.Read())
                {
                    result.Add(dataFromTable[0].ToString());
                }
                dataFromTable.Close();
            }
            this.closeConnection();
            return result;
        }

        public string getIdTraceOrder(string RackNo, string RackPos, string Root)
        {
            string result;
            result = null;
            this.openConnection();

            string querytext = "select max(T0.id_trace_order) from t_trace_order T0 " +
                "join t_trace_order T1 on T0.id_trace_order = T1.id_trace_order and T0.et_order_carord = '11' and T1.et_order_carord = '12' " +
                "join t_trace_order T2 on T0.id_trace_order = T2.id_trace_order and T2.et_order_carord = '-18' " +
                "where T0.trace_order_value = '" + RackNo + "' " +
                "and T1.trace_order_value = '" + RackPos + "' " +
                "and T2.trace_order_value = '" + Root + "'";

            NpgsqlCommand query = new NpgsqlCommand(querytext, Connection);
            NpgsqlDataReader data = query.ExecuteReader();
            // MessageBox.Show(querytext);

            if (data.HasRows & data.Read())
                result = data[0].ToString();
            else
                result = "not found";

            data.Close();
            this.closeConnection();
            return result;
        }


        public List<PartinBom> getFilteredBOM(string id_trace_order, string FamName_t_nomen)
        {
            //List<PartinBom> result = new List<PartinBom>();
            List<PartinBom> BOM_order = new List<PartinBom>();
            //List<PartinBom> BOM_t_nomen = new List<PartinBom>();
           // result = BOM_order;

            BOM_order = this.getSeqBillOfMaterial(id_trace_order);
            //BOM_t_nomen = this.getBillOfMaterial(FamName_t_nomen);
            
            //foreach(PartinBom item1 in BOM_t_nomen)
            //{
            //    foreach(PartinBom item2 in BOM_order)
            //    {
            //        if (item1.PartNumber == item2.PartNumber)
            //        {
            //            result.Add(item1);
            //        }
            //    }
            //}

            return BOM_order;
        }

        public class PartInfo : DatabaseOperations //will also get the batch number if traceability level 2B
        {
            public PartInfo(string id_trace_order)
            {
                // Part.id_trace_order = id_trace_order;
                Part.id_trace_order = id_trace_order;
                Part = this.GeneralInfo();

            }

            public class PartType
            {
                // to represent PN + its description in the list 
                public string Quality { get; set; }
                public string stocpf_no_cli { get; set; }
                public string id_stocpf { get; set; }
                public string id_trace_order { get; set; }
                public string FPPN { get; set; }
                public string Quantity { get; set; }
               
            }

            public PartType Part { get; private set; } = new PartType();


            public PartType GeneralInfo()
            {
                PartType result = new PartType();
                result.id_trace_order = Part.id_trace_order;
                string querytext = "select id_stocpf, stocpf_no_cli, et_qualpf, et_refcmp from t_stocpf where stocpf_numorder = '" + result.id_trace_order +
                    "' order by id_stocpf desc limit 1";

                this.openConnection();

                NpgsqlCommand query = new NpgsqlCommand(querytext, Connection);
                NpgsqlDataReader data = query.ExecuteReader();

                if (data.HasRows & data.Read())
                {
                    result.id_stocpf = data[0].ToString();
                    result.stocpf_no_cli = data[1].ToString();
                    result.Quality = data[2].ToString();
                    result.FPPN = data[3].ToString();
                }

                else
                {
                    result.id_stocpf = "not found";
                    result.stocpf_no_cli = "not found";
                    result.Quality = "not found";
                }

                result.Quantity = this.BOM_quantity();

                data.Close();
                this.closeConnection();
                return result;
            }

            public string getSN(string PartNumber)
            {
                string result = null;
                string querytext = "";
                string TraceabilityLevel = this.getTraceabilityLevel(PartNumber);
                string id_trace_order = Part.id_trace_order;

                switch (TraceabilityLevel)
                {
                    case "U":
                        querytext = "select TP.operat_nscomp from t_stocpf SP " +
                            "join t_operat TP on TP.et_stocpf = SP.id_stocpf and SP.stocpf_numorder = '" + id_trace_order + "' " +
                            "join t_simo TS on TP.et_simo = TS.id_simo " +
                            "join t_nomen TN on TN.id_nomen = TS.et_nomen and TN.et_refakf_comp = '" + PartNumber + "' " +
                            "order by TP.operat_creation desc limit 1 ";

                        break;
                    case "R":
                        querytext = "select TP.operat_nscomp from t_stocpf SP " +
                         "join t_operat TP on TP.et_stocpf = SP.id_stocpf and SP.stocpf_numorder = '" + id_trace_order + "' " +
                         "join t_simo TS on TP.et_simo = TS.id_simo " +
                         "join t_nomen TN on TN.id_nomen = TS.et_nomen and TN.et_refakf_comp = '" + PartNumber + "' " +
                         "order by TP.operat_creation desc limit 1 ";
                        break;
                    case "P":
                        querytext = "select TL.et_lotcmp_nolot from t_stocpf SP " +
                            "join t_lotuse TL on TL.et_stocpf = SP.id_stocpf and SP.stocpf_numorder = '" + id_trace_order + "' " +
                            "and TL.et_ref_comp = '" + PartNumber + "'";
                        break;
                    default:
                        MessageBox.Show("Traceability level not managed!");
                        break;
                }

                this.openConnection();
                NpgsqlCommand query = new NpgsqlCommand(querytext, Connection);
                NpgsqlDataReader data = query.ExecuteReader();

                if (data.HasRows & data.Read())
                    result = data[0].ToString();
                else
                    result = "not found";

                data.Close();
                this.closeConnection();
                return result;
            }

            public string getTraceabilityLevel(string PartNumber)
            {
                string result;
                result = null;
                this.openConnection();

                string querytext = "select distinct et_typtra from t_nomen where et_refakf_comp = '" + PartNumber + "'";

                NpgsqlCommand query = new NpgsqlCommand(querytext, Connection);
                NpgsqlDataReader data = query.ExecuteReader();
                // MessageBox.Show(querytext);

                if (data.HasRows & data.Read())
                    result = data[0].ToString();
                else
                    result = "not found";

                data.Close();
                this.closeConnection();
                return result;
            }

            private string BOM_quantity()
            {
                string result;
                result = null;
                this.openConnection();
                string id_trace_order = Part.id_trace_order;

                string querytext = "select count(*) from t_trace_order where id_trace_order = '" + id_trace_order + "' " +
                    "and et_order_carord = '29'";

                NpgsqlCommand query = new NpgsqlCommand(querytext, Connection);
                NpgsqlDataReader data = query.ExecuteReader();
                // MessageBox.Show(querytext);

                if (data.HasRows & data.Read())
                    result = data[0].ToString();
                else
                    result = "not found";

                data.Close();
                this.closeConnection();
                return result;
            }

        }






        #endregion



    }
}
