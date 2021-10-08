using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Nglib.DATA.COLLECTIONS
{
    public static class DataSetTools
    {




        #region ------- DATAROW -------

        public static List<System.Data.DataRow> ToList(this System.Data.DataRowCollection rows)
        {
            List<System.Data.DataRow> retour = new List<System.Data.DataRow>();
            foreach (System.Data.DataRow item in rows) retour.Add(item);
            return retour;
        }

 


        /// <summary>
        /// Obtenir une donnée d'un datarow et la transformer en string
        /// </summary>
        /// <param name="row"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static string GetRowString(this System.Data.DataRow row, string nameValue)
        {
            try
            {
                object obj = GetRowObject(row, nameValue);
                if (obj == null || obj == DBNull.Value) return string.Empty;
                else return Convert.ToString(obj);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Obtenir une donnée d'un datarow 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="nameValue"></param>
        /// <returns></returns>
        public static object GetRowObject(this System.Data.DataRow row, string nameValue)
        {
            if (row == null) return null;
            try
            {
                System.Data.DataColumn realColumn = GetColumn(row.Table, nameValue);
                if (realColumn == null) return null;
                object obj = row[realColumn];
                if (obj == DBNull.Value) return null;
                return obj;
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// Obtenir une donnée d'un datarow 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="realColumn"></param>
        /// <returns></returns>
        public static object GetRowObject(this System.Data.DataRow row, System.Data.DataColumn realColumn)
        {
            if (row == null) return null;
            if (realColumn == null) return null;

            if (!row.Table.Columns.Contains(realColumn.ColumnName.ToString())) return null;
            if (realColumn != null) return row[realColumn];
            else return null;

        }


        /// <summary>
        /// Obtient une colonne avec gestion de la casse
        /// </summary>
        /// <param name="table"></param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static System.Data.DataColumn GetColumn(this System.Data.DataTable table, string ColumnName)
        {
            return table.Columns[ColumnName];//!!!
        }



        /// <summary>
        /// Savoir si une colonne à été modifiée
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private static bool hasCellChanged(DataRow row, DataColumn col)
        {
            if (!row.HasVersion(DataRowVersion.Original))
            {
                // Row has been added. All columns have changed. 
                return true;
            }
            if (!row.HasVersion(DataRowVersion.Current))
            {
                // Row has been removed. No columns have changed.
                return false;
            }
            var originalVersion = row[col, DataRowVersion.Original];
            var currentVersion = row[col, DataRowVersion.Current];
            if (originalVersion == DBNull.Value && currentVersion == DBNull.Value)
            {
                return false;
            }
            else if (originalVersion != DBNull.Value && currentVersion != DBNull.Value)
            {
                return !originalVersion.Equals(currentVersion);
            }
            return true;
        }




        /// <summary>
        /// Liste des colonnes qui ont été modifiées
        /// </summary>
        public static IEnumerable<DataColumn> GetChangedColumns(this DataRow row)
        {
            return row.Table.Columns.Cast<DataColumn>()
                .Where(col => hasCellChanged(row, col));
        }


        /// <summary>
        /// Liste des colonnes qui ont été modifiées
        /// </summary>
        public static string[] GetChangedColumnNames(this DataRow row)
        {
            return GetChangedColumns(row).Select(c => c.ColumnName).ToArray();
        }

        /// <summary>
        /// Liste des colonnes qui ont été modifiées
        /// </summary>
        public static IEnumerable<DataColumn> GetChangedColumns(this IEnumerable<DataRow> rows)
        {
            return rows.SelectMany(row => row.GetChangedColumns())
                .Distinct();
        }


        /// <summary>
        /// Liste des colonnes qui ont été modifiées
        /// </summary>
        public static string[] GetChangedColumnNames(this IEnumerable<DataRow> rows)
        {
            return GetChangedColumns(rows).Select(c => c.ColumnName).ToArray();
        }


        /// <summary>
        /// Liste des colonnes qui ont été modifiées
        /// </summary>
        public static IEnumerable<DataColumn> GetChangedColumns(this DataTable table)
        {
            return table.GetChanges().Rows
                .Cast<DataRow>()
                .GetChangedColumns();
        }


        /// <summary>
        /// Liste des colonnes qui ont été modifiées
        /// </summary>
        public static string[] GetChangedColumnNames(this DataTable table)
        {
            return GetChangedColumns(table).Select(c => c.ColumnName).ToArray();
        }



        /// <summary>
        /// Liste des données qui on été modifiée
        /// </summary>
        /// <param name="row">row</param>
        /// <param name="NotReturnKey">ne retournera pas les primarykeys</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetChangedValues(this DataRow row, bool NotReturnKey = true)
        {
            if (row == null) return null;
            Dictionary<string, object> retour = new Dictionary<string, object>();
            List<System.Data.DataColumn> colKeys = row.Table.PrimaryKey.ToList();
            foreach (var itemcol in GetChangedColumns(row))
            {
                if (NotReturnKey && colKeys.Contains(itemcol)) continue;
                object val = GetRowObject(row, itemcol);
                retour.Add(itemcol.ColumnName, val);
            }
            return retour;
        }


        /// <summary>
        /// Obtient les données d'un datarow sous forme de dictionnaire
        /// </summary>
        /// <param name="row">row</param>
        /// <param name="getColKeys">retour les primary ou non</param>
        /// <param name="getColValues">retourne les données autre que primarykey ou non</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(this System.Data.DataRow row, bool includeKeys, bool includeValues)
        {
            if (row == null) return null;
            Dictionary<string, object> retour = new Dictionary<string, object>();
            List<System.Data.DataColumn> colKeys = null;
            if (row.Table.PrimaryKey != null) colKeys = row.Table.PrimaryKey.ToList();
            else colKeys = new List<DataColumn>();
            if (includeKeys)
            {
                foreach (System.Data.DataColumn itemcol in colKeys)
                    retour.Add(itemcol.ColumnName, row[itemcol.ColumnName]);
            }
            if (includeValues)
            {
                foreach (System.Data.DataColumn itemcol in row.Table.Columns)
                {
                    if (colKeys.Contains(itemcol)) continue;
                    retour.Add(itemcol.ColumnName, row[itemcol.ColumnName]);
                }
            }
            return retour;
        }


        /// <summary>
        /// Obtient les données d'un datarow sous forme de dictionnaire
        /// </summary>
        /// <param name="row">Données</param>
        /// <param name="colNames">Selection de colonnes</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetValues(this System.Data.DataRow row, params string[] colFilters)
        {
            if (row == null) return null;
            try
            {
                Dictionary<string, object> retour = new Dictionary<string, object>();
                foreach (System.Data.DataColumn itemcol in row.Table.Columns)
                {
                    if(colFilters != null && colFilters.Length!=0) // si des colones particulières sont demandées
                        if ( colFilters.FirstOrDefault(c => c.Equals(itemcol.ColumnName, StringComparison.OrdinalIgnoreCase)) == null) continue; // col non demandé
                    retour.Add(itemcol.ColumnName, row[itemcol.ColumnName]);
                }
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("GetValues " + ex.Message);
            }
        }


        /// <summary>
        ///  Permet de clonner le datarow
        /// </summary>
        /// <param name="row">datarow</param>
        /// <param name="ClonneDataTableToo">Clone l'objet aussi dans une nouvelle table clonée</param>
        /// <returns></returns>
        public static System.Data.DataRow CloneRow(this System.Data.DataRow row, bool ClonneDataTableToo = false)
        {
            try
            {
                if (row == null) return null;
                System.Data.DataRow retour = null;
                if (ClonneDataTableToo) retour = row.Table.Clone().NewRow();
                else retour = row.Table.NewRow();
                retour.ItemArray = row.ItemArray;
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("CloneRow " + ex.Message);
            }
        }


        /// <summary>
        /// Permet de copier les données données d'un datarow dans un autres selon les colones
        /// </summary>
        public static void CopyRow(this System.Data.DataRow row,  System.Data.DataRow rowDestination)
        {
            foreach (System.Data.DataColumn col in row.Table.Columns)
            {
                System.Data.DataColumn colDestination = rowDestination.Table.GetColumn(col.ColumnName);
                if (colDestination == null) continue;
                rowDestination[colDestination] = row[col];
            }
        }



        /// <summary>
        /// Permet de changer le datatable d'un row
        /// Clone et recré un datarow avec des données déja présente à l'origine en prenant en compte les contraintes de la nouvelle table
        /// </summary>
        /// <param name="orgnRow"></param>
        /// <param name="NewTable"></param>
        /// <returns></returns>
        public static System.Data.DataRow CloneRowInOtherTable(System.Data.DataRow orgnRow, System.Data.DataTable NewTable)
        {
            NewTable.CaseSensitive = false; // pas de c
            orgnRow.Table.CleanDataTableSchema(); // to lower pour éviter que le merge duplique des colone en case sensitive
            NewTable.CleanDataTableSchema();// to lower pour éviter que le merge duplique des colone en case sensitive
            System.Data.DataRow newRow = NewTable.NewRow();


            if (orgnRow != null) // Faut recoppier les données d'origine et eventuellement créer les colonnes manquantes dans la nouvelle table
            {
                // on lance un merge de l'ancienne sur la nouvelle au cas, il y aura des colones qui aurai disparu sur la nouvelle
                //NewTable.Merge(orgnRow.Table, true, MissingSchemaAction.Add); // le merge standard marche pas (il duplique les clef casesensitive)

                NewTable.MergeAddSchema(orgnRow.Table);

                // Copie données col par col
                foreach (System.Data.DataColumn orgnCol in orgnRow.Table.Columns)
                    newRow[orgnCol.ColumnName] = orgnRow[orgnCol.ColumnName];
            }


            if (orgnRow.RowState != DataRowState.Detached) // était attaché avant alors on l'attache
            {
            }
            return newRow;
        }






        /// <summary>
        /// Permet de clonner les données des datarow en une seule nouvelle datatable
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static System.Data.DataTable CloneRowsInNewDataTable(params System.Data.DataRow[] rows)
        {

            try
            {
                List<System.Data.DataTable> alltables = rows.Select(dt => dt.Table).Distinct().ToList();
                System.Data.DataTable tabinsert = DataTableMergeSchemas(alltables.ToArray());
                DataTableRemoveConstraints(tabinsert);
                foreach (System.Data.DataRow row in rows)
                {
                    System.Data.DataRow newrow = tabinsert.NewRow();
                    foreach (System.Data.DataColumn itemcol in tabinsert.Columns)
                    {
                        object obj = GetRowObject(row, itemcol);
                        if (obj == null) newrow[itemcol] = DBNull.Value;
                        else newrow[itemcol] = obj;

                    }
                    tabinsert.Rows.Add(newrow);


                    //System.Data.DataRow srcrow = item.GetRow();
                    //tabinsert.Merge(srcrow.Table, true, System.Data.MissingSchemaAction.Add); // on merge les deux structure de tables, car les po peuvent créer des colones séparément

                    // la fonction merge, merge aussi les données, mais quand un datapo n'a pas été ajouté en base (datarow detached), il ne figure pas dans les datatable
                    //tabinsert.Rows.Add(srcrow.ItemArray); // on Ajoute donc la nouvelle ligne

                }
                return tabinsert;
            }
            catch (Exception ex)
            {
                throw new Exception("MergeInOneDataTable " + ex.Message);
            }
        }






        #endregion














        #region ------------ DATATABLE Tools

        /// <summary>
        /// Permet de créer ou définir une table
        /// </summary>
        public static System.Data.DataTable DefineDataTable(string nameTable, System.Data.DataColumn[] Cols, System.Data.DataColumn[] ColsKeys, System.Data.DataTable orgntable = null)
        {
            if (orgntable == null) orgntable = new DataTable();


            // renomme la table
            orgntable.TableName = nameTable;

            DataSetTools.SetColumns(orgntable,Cols);


            // défini les clefs primaires
            if (ColsKeys != null)
                DataSetTools.SetPrimaryKeys(orgntable, ColsKeys);

            return orgntable;
        }


        /// <summary>
        /// Permet de nétoyer une datatable pour la rendre compatible a certaine fonction
        /// </summary>
        /// <param name="table"></param>
        public static void CleanDataTableSchema(this System.Data.DataTable table)
        {
            table.CaseSensitive = false;
            foreach (System.Data.DataColumn item in table.Columns)
            {
                item.ColumnName = item.ColumnName.ToLower(); // 
            }
        }


        /// <summary>
        /// Permet d'additionner le schémas tableSource dans tableDest
        /// </summary>
        /// <param name="tableDest"></param>
        /// <param name="tableSource"></param>
        public static void MergeAddSchema(this System.Data.DataTable tableDest, System.Data.DataTable tableSource)
        {
            tableDest.CleanDataTableSchema();
            tableSource.CleanDataTableSchema();

            tableDest.TableName = tableSource.TableName;
   
            // ajoute les colonnes manquantes
            foreach (System.Data.DataColumn colSrc in tableSource.Columns)
            {
                if (tableDest.Columns.Contains(colSrc.ColumnName)) continue; // existe déja
                System.Data.DataColumn colDest = colSrc.CloneColumn();
                tableDest.Columns.Add(colDest);
            }

            // ajoute les même pk manquantes
            List<string> tableDestpks = tableDest.PrimaryKey.Select(pk => pk.ColumnName).ToList();
            bool modified = false;
            foreach (System.Data.DataColumn colpkSrc in tableSource.PrimaryKey)
            {
                if (tableDestpks.Contains(colpkSrc.ColumnName)) continue;
                tableDestpks.Add(colpkSrc.ColumnName);
                modified = true;
            }
            if (modified) tableDest.SetPrimaryKeys(tableDestpks.ToArray());


            // ajoute les contraintes
            //tableDest.Constraints  !!!

        }

        /// <summary>
        /// Permet de clonner une colonne
        /// </summary>
        /// <param name="colOrgn"></param>
        /// <returns></returns>
        public static System.Data.DataColumn CloneColumn(this System.Data.DataColumn colOrgn)
        {
            System.Data.DataColumn colDest = new DataColumn(colOrgn.ColumnName.ToLower().Trim());
            colDest.AllowDBNull = colOrgn.AllowDBNull;
            colDest.AutoIncrement = colOrgn.AutoIncrement;
            colDest.MaxLength = colOrgn.MaxLength; 
            colDest.ReadOnly = colOrgn.ReadOnly;
            colDest.Unique = colOrgn.Unique;

            return colDest;
        }


        /// <summary>
        /// permet de créer une colonne sur la tables
        /// </summary>
        /// <param name="table"></param>
        /// <param name="nameCol"></param>
        /// <param name="type"></param>
        /// <param name="IsKey"></param>
        /// <returns></returns>
        public static System.Data.DataColumn CreateColumn(this System.Data.DataTable table, string nameCol, Type type, bool IsKey = false)
        {
            System.Data.DataColumn retour = null;
            try
            {
                if (table == null) throw new Exception("table null");
                if (string.IsNullOrWhiteSpace(nameCol)) throw new Exception("nameCol empty");
                if (type == null || type == typeof(DBNull)) throw new Exception("type empty");// impossible de créer un type null
                nameCol = nameCol.Trim().ToLower(); // propre
                retour = table.Columns[nameCol];
                if (retour != null) return null; // existe déja ...



                retour = new System.Data.DataColumn(nameCol, type);
                table.Columns.Add(retour);


                if (IsKey)
                    SetPrimaryKeys(table, retour);
 
                return retour;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {

            }
        }


        /// <summary>
        /// Permet de fusionner tous les schémas en une table
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static System.Data.DataTable DataTableMergeSchemas(params System.Data.DataTable[] tables)
        {
            if (tables == null || tables.Length == 0) return null;
            //if (tables.Length == 1) return tables[0];
            System.Data.DataTable retour = tables.FirstOrDefault().Clone();

            foreach (var itemtable in tables) // parcour des tables
            {

                var clontable = itemtable.Clone();
                retour.Merge(clontable, true, System.Data.MissingSchemaAction.Add);

                //foreach (var itemcol in itemtable.Columns)
                //{
                //    if(itemtable.Columns.Contains()
                //}


            }

            return retour;
        }


        /// <summary>
        /// Supprimer toutes les contraintes sur un datatable
        /// </summary>
        /// <param name="table"></param>
        public static void DataTableRemoveConstraints(this System.Data.DataTable table)
        {
            if (table == null) return;
            table.PrimaryKey = null;
            table.Constraints.Clear();
            foreach (System.Data.DataColumn itemcol in table.Columns)
            {
                itemcol.AllowDBNull = true;
                itemcol.AutoIncrement = false;
                itemcol.Unique = false;
                //itemcol.MaxLength = 0;
            }

        }


        /// <summary>
        /// définir des colones comme primary key
        /// en reprenant bien celle de la table si existe déja
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyColsNames"></param>
        /// <returns></returns>
        public static void SetPrimaryKeys(this DataTable table, params System.Data.DataColumn[] colsPrimKeys)
        {
            System.Data.DataColumn[] addkeys = SetColumns(table, colsPrimKeys);
            table.PrimaryKey = addkeys.ToArray();
        }

        /// <summary>
        /// définir des colones comme primary key
        /// Les colonnes doivent déja existées
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyColsNames"></param>
        /// <returns></returns>
        public static void SetPrimaryKeys(this DataTable table, params string[] keyColsNames)
        {
            try
            {
                List<System.Data.DataColumn> colsPrimKeys = new List<System.Data.DataColumn>();
                foreach (string primaryJey in keyColsNames)
                {
                    if (string.IsNullOrWhiteSpace(primaryJey)) continue;
                    foreach (System.Data.DataColumn retsourcecol in table.Columns)
                        if (retsourcecol.ColumnName == primaryJey) { colsPrimKeys.Add(retsourcecol); break; }
                }
                table.PrimaryKey = colsPrimKeys.ToArray();

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Ajoute de nouvelle colones sur la table si elle existe pas déja
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <returns>retournera lse vraie objet des colones sur la table</returns>
        public static System.Data.DataColumn[] SetColumns(this DataTable table, params System.Data.DataColumn[] columns)
        {
            if (columns == null) return null;
            List<System.Data.DataColumn> columnsintable = new List<DataColumn>();
           

            foreach (System.Data.DataColumn col in columns)
            {
                System.Data.DataColumn colreal = GetColumn(table, col.ColumnName);
                if (colreal != null) columnsintable.Add(colreal);
                else
                {
                    table.Columns.Add(col); // il faut ajouter au colone standard de la table avant de la mettre dans les primarykey
                    columnsintable.Add(col);
                }
            }
            return columnsintable.ToArray();
        }



        /// <summary>
        /// Obtenir plusieurs colonnes
        /// </summary>
        /// <param name="table"></param>
        /// <param name="colsnames"></param>
        /// <returns></returns>
        public static System.Data.DataColumn[] GetColumns(this System.Data.DataTable table, params string[] colsnames)
        {
            List<System.Data.DataColumn> retour = new List<DataColumn>();
            List<string> colsnamesl = colsnames.ToList();
            foreach (System.Data.DataColumn item in table.Columns)
            {
                if (colsnames != null && colsnames.Length > 0 && colsnamesl.Count(c => c.Equals(item.ColumnName, StringComparison.OrdinalIgnoreCase))==0) continue; // pas demandé
                retour.Add(item);
            }
            return retour.ToArray();
        }



        public static List<System.Data.DataColumn> ToList(this System.Data.DataColumnCollection columns)
        {
            List<System.Data.DataColumn> retour = new List<System.Data.DataColumn>();
            foreach (System.Data.DataColumn item in columns) retour.Add(item);
            return retour;
        }


        /// <summary>
        /// Découpe un datatable en plusieurs datatable et avec clonnage des données
        /// </summary>
        /// <param name="retSrc"></param>
        /// <param name="countbyitem"></param>
        /// <param name="removeColumn"></param>
        /// <returns></returns>
        public static List<System.Data.DataTable> DataTableSplit(this System.Data.DataTable retSrc, int countbyitem = 100)
        {
            try
            {
                List<System.Data.DataTable> tablesliteds = new List<System.Data.DataTable>();

                System.Data.DataTable newtable = null;
                int ii = 0;

                foreach (System.Data.DataRow row in retSrc.Rows)
                {
                    // Création d'une nouvelle table nécessaire pour ajouter la ligne si existe pas
                    if (newtable == null)
                    {
                        newtable = retSrc.Clone();
                        newtable.TableName = retSrc.TableName;
                        tablesliteds.Add(newtable);
                    }

                    // clone row
                    newtable.Rows.Add(row.ItemArray);
                    ii++;


                    if (ii >= countbyitem) // si on as attein le nombre de ligne maximum sur cette table, 
                    {
                        newtable = null; // forcera la prochaine ligne à etre ajouté sur une autre table
                        ii = 0;
                    }
                }

                return tablesliteds;
            }
            catch (Exception)
            {

                throw;
            }
        }




        /// <summary>
        /// Extrait deux données d'une table dans un dictionary
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="ColKey"></param>
        /// <param name="ColValue"></param>
        /// <param name="IgnoreDuplicate"></param>
        /// <returns></returns>
        public static Dictionary<string, string> DataTableToDictionaryValues(this System.Data.DataTable ret, string ColKey, string ColValue, bool IgnoreDuplicate = false)
        {
            Dictionary<string, string> retour = new Dictionary<string, string>();
            foreach (System.Data.DataRow row in ret.Rows)
            {


                string ckey = GetRowString(row, ColKey);
                if (string.IsNullOrWhiteSpace(ckey)) continue;
                if (retour.ContainsKey(ckey))
                {
                    if (IgnoreDuplicate) continue;
                    else throw new Exception("ToDictionaryValue duplicate ColKey");
                }
                string cval = GetRowString(row, ColValue);
                retour.Add(ckey, cval);
            }
            return retour;
        }


        /// <summary>
        /// Extrait une données d'une table dans une liste
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="ColKey"></param>
        /// <returns></returns>
        public static List<string> DataTableToListValues(this System.Data.DataTable ret, string ColKey)
        {
            List<string> retour = new List<string>();
            System.Data.DataColumn col = GetColumn(ret, ColKey);
            foreach (System.Data.DataRow row in ret.Rows)
            {
                object valkey = row[col];
                retour.Add(Convert.ToString(valkey));
            }
            return retour;
        }


        /// <summary>
        /// Séparer les tables avec une colones clef
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="ColKey"></param>
        /// <returns></returns>
        public static Dictionary<string, System.Data.DataTable> DataTableSplitByKey(this System.Data.DataTable ret, string ColKey)
        {
            Dictionary<string, System.Data.DataTable> retour = new Dictionary<string, System.Data.DataTable>();
            foreach (System.Data.DataRow row in ret.Rows)
            {
                System.Data.DataTable rtable = null;
                string ckey = GetRowString(row, ColKey);
                if (!retour.ContainsKey(ckey))
                {
                    rtable = ret.Clone();
                    retour.Add(ckey, rtable);
                }
                else rtable = retour[ckey];
                rtable.Rows.Add(row.ItemArray);
            }
            return retour;
        }






  /*
        /// <summary>
        /// Gets a Inverted DataTable
        /// https://www.codeproject.com/Articles/22008/C-Pivot-Table
        /// </summary>
        /// <param name="table">Provided DataTable</param>
        /// <param name="columnX">X Axis Column</param>
        /// <param name="columnY">Y Axis Column</param>
        /// <param name="columnZ">Z Axis Column (values)</param>
        /// <param name="columnsToIgnore">Whether to ignore some column, it must be 
        /// provided here</param>
        /// <param name="nullValue">null Values to be filled</param> 
        /// <returns>C# Pivot Table Method  - Felipe Sabino</returns>
        public static DataTable PivotInversedTable(DataTable table, string columnX,
             string columnY, string columnZ, string nullValue, bool sumValues)
        {
            //Verify if Y and Z Axis columns re provided
            if (string.IsNullOrEmpty(columnY) || string.IsNullOrEmpty(columnZ))
                throw new Exception("The columns to perform inversion are not provided");

            //Create a DataTable to Return
            DataTable returnTable = new DataTable();
            if (columnX == "") columnX = table.Columns[0].ColumnName;
            returnTable.Columns.Add(columnY);
            //Read all DISTINCT values from columnX Column in the provided DataTale
            List<string> columnXValues = table.Rows.ToList().Select(row => Nglib.DATA.COLLECTIONS.DataSetTools.GetRowString(row, columnX)).Distinct().ToList();


            //Read DISTINCT Values for Y Axis Column
            List<string> columnYValues = new List<string>();
            foreach (DataRow dr in table.Rows)
                if (!columnYValues.Contains(dr[columnY].ToString()))
                    columnYValues.Add(dr[columnY].ToString());


            //Loop all Column Y Distinct Value
            foreach (string columnYValue in columnYValues)
            {
                //Creates a new Row
                DataRow drReturn = returnTable.NewRow();
                drReturn[0] = columnYValue;
                //foreach column Y value, The rows are selected distincted
                DataRow[] rows = table.Select(columnY + "='" + columnYValue + "'");

                //Read each row to fill the DataTable
                foreach (DataRow dr in rows)
                {
                    string rowColumnTitle = dr[columnX].ToString();

                    //Read each column to fill the DataTable
                    foreach (DataColumn dc in returnTable.Columns)
                    {
                        if (!dc.ColumnName.Equals(rowColumnTitle))
                            continue;

                        //If Sum of Values is True it try to perform a Sum
                        //If sum is not possible due to value types, the value 
                        // displayed is the last one read
                        if (sumValues)
                        {
                            try
                            {
                                drReturn[rowColumnTitle] =
                                     Convert.ToDecimal(drReturn[rowColumnTitle]) +
                                     Convert.ToDecimal(dr[columnZ]);
                            }
                            catch
                            {
                                drReturn[rowColumnTitle] = dr[columnZ];
                            }
                        }
                        else
                        {
                            drReturn[rowColumnTitle] = dr[columnZ];
                        }

                    }
                }
                returnTable.Rows.Add(drReturn);
            }


            //if a nullValue is provided, fill the datable with it
            if (nullValue != "")
                foreach (DataRow dr in returnTable.Rows)
                    foreach (DataColumn dc in returnTable.Columns)
                        if (dr[dc.ColumnName].ToString() == "")
                            dr[dc.ColumnName] = nullValue;


            return returnTable;
        }
  */

        #endregion













        public static string SerializeDataTable(this System.Data.DataTable ret, System.Data.XmlWriteMode shema = System.Data.XmlWriteMode.WriteSchema)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    ret.WriteXml(stream, shema);
                    stream.Position = 0;
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SerializeDataTable " + ex.Message, ex);
            }

        }

        public static string SerializeDataRow(this System.Data.DataRow row, System.Data.XmlWriteMode shema = System.Data.XmlWriteMode.WriteSchema)
        {
            System.Data.DataTable tabletosend = null;
            if (row.Table.Rows.Count > 1)
            {
                tabletosend = row.Table.Clone();
                tabletosend.Rows.Add(row);
            }
            else tabletosend = row.Table;
            return SerializeDataTable(tabletosend, shema);
        }



        public static System.Data.DataTable DeSerializeDataTable(string Contentdata)
        {
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(Contentdata);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    return DeSerializeDataTable(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DeSerializeDataTable " + ex.Message, ex);
            }
        }


        public static System.Data.DataTable DeSerializeDataTable(Stream Contentdata)
        {
            try
            {
                System.Data.DataTable retour = new System.Data.DataTable();
                retour.ReadXml(Contentdata);
                return retour;
            }
            catch (Exception ex)
            {
                throw new Exception("DeSerializeDataTable " + ex.Message, ex);
            }
        }




        /// <summary>
        ///  Extraire les données sous forme de CSV
        /// </summary>
        /// <param name="dtValuesdata">table</param>
        /// <param name="colFilters">filtre sur des colonnes</param>
        /// <param name="WriteHeader">Ajouter un Header</param>
        /// <returns></returns>
        public static string[] DatatableToCSV(this System.Data.DataTable dtValuesdata, string[] colFilters = null, bool WriteHeader=false)
        {
            if (dtValuesdata == null) return null;
            

            try
            {
                List<string> retour = new List<string>();

                // header
                if(WriteHeader)
                {
                    List<string> colsnames = new List<string>();
                    foreach (System.Data.DataColumn col in dtValuesdata.Columns)
                    {
                        if (colFilters != null && colFilters.FirstOrDefault(c => c.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase )) == null) continue; // col non demandée
                        colsnames.Add(col.ColumnName);
                    }
                    retour.Add(string.Join(";", colsnames.ToArray()));
                }

                //data
                foreach (System.Data.DataRow row in dtValuesdata.Rows)
                {
                    Dictionary<string, object> vals = GetValues(row, colFilters);
                    // on prend les prends les données et on efface ce qui pourrai géner la mise en forme CSV
                    string[] fieldsline = vals.Select(val => (val.Value == null) ? null : Convert.ToString(val.Value).Replace(";", "").Replace("\r", "").Replace("\n", "")).ToArray();
                    retour.Add(string.Join(";", fieldsline.ToArray()));
                }

                return retour.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("DatatableToCSV "+ex.Message,ex);
            }
        }






    }
}
