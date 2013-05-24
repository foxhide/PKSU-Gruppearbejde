using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

using CalendarApplication.Models.Group;
using System.Data;

namespace CalendarApplication.Database
{
    public class MySqlGroup : MySqlConnect
    {
        /// <summary>
        /// Create a new group -WIP
        /// </summary>
        /// <param name="groupmodel">Model with data to be stored</param>
        /// <returns>bool indicating success or failure</returns>
        public bool CreateGroup(GroupModel groupmodel)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                cmd = new MySqlCommand();

                try
                {
                    mst = connection.BeginTransaction();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    //body
                    cmd.CommandText = "INSERT INTO groups (groupName) VALUES (@groupName); SELECT last_insert_id()";
                    cmd.Parameters.AddWithValue("@groupName", groupmodel.Name);
                    cmd.Prepare();
                    int id = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd.CommandText = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @groupLeader, @canCreate)";
                    cmd.Parameters.AddWithValue("@groupId", groupmodel.ID);
                    cmd.Parameters.AddWithValue("@userId", null);
                    cmd.Parameters.AddWithValue("@groupLeader", 0);
                    cmd.Parameters.AddWithValue("@canCreate", 0);
                    cmd.Prepare();

                    for (int i = 0; i < groupmodel.groupMembers.Count; i++)
                    {
                        if (groupmodel.groupMembers[i].Selected)
                        {
                            
                            cmd.Parameters["@userId"].Value = groupmodel.groupMembers[i].Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    mst.Commit();

                    this.CloseConnection();
                    return true;
                }
                catch (MySqlException ex)
                {
                    this.ErrorMessage = ex.Message + " caused by: " + cmd.CommandText;
                    mst.Rollback();
                    this.CloseConnection();
                    return false;
                }
            }
            else
            {
                //could not open connection
                return false;
            }
        }

        public static GroupModel getGroup(int groupId)
        {
            GroupModel result = new GroupModel { ID = groupId };

            MySqlConnect msc = new MySqlConnect();
            string cmd = "SELECT * FROM groups WHERE groupId = @groupId";
            string[] argnames = { "@groupId" };
            object[] args = { groupId };
            DataRow dr = msc.ExecuteQuery(new CustomQuery { Cmd = cmd, ArgNames = argnames, Args = args }).Rows[0];
            
            result.Name = (string)dr["groupName"];

            return result;
        }

        /// <summary>
        /// Edit an existing group
        /// </summary>
        /// <param name="groupmodel"></param>
        /// <returns>bool indicating success or failure</returns>
        public bool EditGroup(GroupModel groupmodel)
        {
            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;
                cmd = new MySqlCommand();

                try
                {
                    mst = connection.BeginTransaction();
                    cmd.Connection = connection;
                    cmd.Transaction = mst;

                    cmd.CommandText = "UPDATE groups SET groupName = @groupName WHERE groupId = @groupId";
                    cmd.Parameters.AddWithValue("@groupName", groupmodel.Name);
                    cmd.Parameters.AddWithValue("@groupId", groupmodel.ID);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM groupmembers WHERE groupId = @groupId";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    if (groupmodel.groupMembers == null)
                    {
                        groupmodel.groupMembers = new List<SelectListItem>();
                    }
                    if (groupmodel.groupLeaders == null)
                    {
                        groupmodel.groupLeaders = new List<SelectListItem>();
                    }

                    int memberSize = groupmodel.groupMembers.Count;
                    int leaderSize = groupmodel.groupLeaders.Count;
                    
                    cmd.CommandText = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @groupLeader, @canCreate)";
                    cmd.Parameters.AddWithValue("@userId", null);
                    cmd.Parameters.AddWithValue("@groupLeader", 0);
                    cmd.Parameters.AddWithValue("@canCreate", 0);
                    cmd.Prepare();

                    for (int i = 0; i < memberSize; i++)
                    {
                        if (groupmodel.groupMembers[i].Selected)
                        {
                            cmd.Parameters["@userId"].Value = groupmodel.groupMembers[i].Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    cmd.CommandText = "UPDATE groupmembers SET groupLeader = @groupLeader, canCreate = @canCreate WHERE userId = @userId";
                    cmd.Prepare();

                    for (int i = 0; i < leaderSize; i++)
                    {
                        cmd.Parameters["@groupLeader"].Value = groupmodel.groupLeaders[i].Selected;
                        cmd.Parameters["@canCreate"].Value = groupmodel.groupLeaders[i].Selected || groupmodel.canCreate[i].Selected;
                        cmd.Parameters["@userId"].Value = groupmodel.groupLeaders[i].Value;
                        cmd.ExecuteNonQuery();
                    }

                    mst.Commit();

                    this.CloseConnection();
                    return true;
                }
                catch (MySqlException ex)
                {
                    this.ErrorMessage = ex.Message + " caused by: " + cmd.CommandText;
                    mst.Rollback();
                    this.CloseConnection();
                    return false;
                }
            }
            else
            {
                //could not open connection
                return false;
            }
        }
    }
}