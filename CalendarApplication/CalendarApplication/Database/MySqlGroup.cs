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
        /// Create a new group
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
                    cmd.CommandText = "INSERT INTO groups (groupName,open) VALUES (@groupName,@open); SELECT last_insert_id()";
                    cmd.Parameters.AddWithValue("@groupName", groupmodel.Name);
                    cmd.Parameters.AddWithValue("@open", groupmodel.Open);
                    cmd.Prepare();
                    int id = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd.CommandText = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @groupLeader, @canCreate)";
                    cmd.Parameters.AddWithValue("@groupId", id);
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
            result.Open = (bool)dr["open"];

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

                    cmd.CommandText = "UPDATE groups SET groupName = @groupName, open = @open WHERE groupId = @groupId";
                    cmd.Parameters.AddWithValue("@groupName", groupmodel.Name);
                    cmd.Parameters.AddWithValue("@groupId", groupmodel.ID);
                    cmd.Parameters.AddWithValue("@open", groupmodel.Open);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM groupmembers WHERE groupId = @groupId";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    if (groupmodel.Open)
                    {
                        // delete any applicants if open (might have been closed before, negligible cost if not)
                        cmd.CommandText = "DELETE FROM groupapplicants WHERE groupId = @groupId";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    if (groupmodel.groupMembers == null)
                    {
                        groupmodel.groupMembers = new List<SelectListItem>();
                    }
                    if (groupmodel.groupLeaders == null)
                    {
                        groupmodel.groupLeaders = new List<SelectListItem>();
                    }
                    if (groupmodel.canCreate == null)
                    {
                        groupmodel.canCreate = new List<SelectListItem>();
                    }

                    int memberSize = groupmodel.groupMembers.Count;
                    int leaderSize = groupmodel.groupLeaders.Count;
                    
                    cmd.Parameters.AddWithValue("@userId", null);
                    cmd.Parameters.AddWithValue("@groupLeader", false);
                    cmd.Parameters.AddWithValue("@canCreate", false);

                    for (int i = 0; i < memberSize; i++)
                    {
                        if (groupmodel.groupMembers[i].Selected)
                        {
                            cmd.CommandText = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @groupLeader, @canCreate)";
                            cmd.Parameters["@userId"].Value = int.Parse(groupmodel.groupMembers[i].Value);
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();

                            //make sure to delete applications just in case they were applicants before
                            cmd.CommandText = "DELETE FROM groupapplicants WHERE userId = @userId AND groupId = @groupId";
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();

                        }
                    }

                    cmd.CommandText = "UPDATE groupmembers SET groupLeader = @groupLeader, canCreate = @canCreate WHERE userId = @userId AND groupId = @groupId";
                    cmd.Prepare();

                    for (int i = 0; i < leaderSize; i++)
                    {
                        cmd.Parameters["@groupLeader"].Value = groupmodel.groupLeaders[i].Selected;
                        cmd.Parameters["@canCreate"].Value = groupmodel.groupLeaders[i].Selected || groupmodel.canCreate[i].Selected;
                        cmd.Parameters["@userId"].Value = int.Parse(groupmodel.groupLeaders[i].Value);
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

        /// <summary>
        /// Set privileges for group from GroupPrivilegesModel
        /// </summary>
        /// <param name="model">Model to set privileges from</param>
        /// <returns>bool indicating success or failure</returns>
        public bool SetPrivileges(GroupPrivilegesModel model)
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

                    cmd.CommandText = "DELETE FROM eventcreationgroups WHERE groupId = @groupId";
                    cmd.Parameters.AddWithValue("@groupId", model.ID);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO eventcreationgroups (groupId, eventTypeId) VALUES (@groupId, @eventTypeId)";
                    cmd.Parameters.AddWithValue("@eventTypeId", null);
                    cmd.Prepare();

                    for (int i = 0; i < model.EventTypes.Count; i++)
                    {
                        if (model.EventTypes[i].Selected)
                        {
                            cmd.Parameters["@eventTypeId"].Value = int.Parse(model.EventTypes[i].Value);
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


        /// <summary>
        /// Delete existing group
        /// </summary>
        /// <param name="id">group id</param>
        /// <returns>bool indicating success or failure</returns>
        public bool DeleteGroup(int id)
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
                    
                    cmd.CommandText = "DELETE FROM groups WHERE groupId = @groupId";
                    cmd.Parameters.AddWithValue("@groupId", id);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

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

        /// <summary>
        /// Handles applications to groups from list of groups and applicants
        /// </summary>
        /// <param name="model">ApplicantListModel for all applicants</param>
        /// <returns>bool indicating success or failure</returns>
        public bool HandleApplications(ApplicantListModel model)
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
                    cmd.Parameters.AddWithValue("@groupId", null);
                    cmd.Parameters.AddWithValue("@userId", null);
                    cmd.Parameters.AddWithValue("@create", null);
                    cmd.Parameters.AddWithValue("@leader", null);
                    foreach (ApplicantListModel.ApplicantGroupModel group in model.ApplicationGroupList)
                    {
                        cmd.Parameters["@groupId"].Value = group.GroupID;
                        foreach (ApplicantListModel.ApplicantGroupModel.ApplicantModel appl in group.ApplicantList)
                        {
                            string command = "";

                            if (appl.Delete)
                            {
                                command = "DELETE FROM groupapplicants WHERE userId = @userId AND groupId = @groupId";
                                cmd.Parameters["@userId"].Value = appl.UserID;

                                cmd.CommandText = command;

                                cmd.Prepare();
                                cmd.ExecuteNonQuery();

                            }
                            else
                            if (appl.Accept)
                            {
                                command = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @leader, @create)";
                                cmd.Parameters["@userId"].Value = appl.UserID;
                                cmd.Parameters["@create"].Value = appl.MakeLeader || appl.MakeCreator;
                                cmd.Parameters["@leader"].Value = appl.MakeLeader;
                                cmd.CommandText = command;

                                cmd.Prepare();
                                cmd.ExecuteNonQuery();

                                // remove application when accepted
                                command = "DELETE FROM groupapplicants WHERE userId = @userId AND groupId = @groupId";
                                cmd.CommandText = command;

                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }
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

        /// <summary>
        /// Allows a single user to apply to a group, by id.
        /// If group is open, simply adds the user to the group
        /// </summary>
        /// <param name="userId">ID of applicant</param>
        /// <param name="groupId">ID of group</param>
        /// <param name="open">Whether group is open</param>
        /// <returns>bool indicating success or failure</returns>
        public bool ApplyToGroup(int userId, int groupId, bool open)
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
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@create", false);
                    cmd.Parameters.AddWithValue("@leader", false);
                    if (open)
                    {
                        cmd.CommandText = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @leader, @create)";
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO groupapplicants(groupId, userId) VALUES (@groupId, @userId)";
                        cmd.Prepare();
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

        /// <summary>
        /// Gets count of all applications to groups
        /// If a user occurs more than once in group applications table, all entries are counted
        /// </summary>
        /// <returns>count of all applcations to groups</returns>
        public int GetApplicantCountAdmin()
        {
            int result = -1;

            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

                try
                {
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
                    cmd.Transaction = mst;

                    string count = "SELECT COUNT(*) FROM groupapplicants";

                    cmd.CommandText = count;
                    cmd.Prepare();

                    result = Convert.ToInt32(cmd.ExecuteScalar());
                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    this.CloseConnection();
                    ErrorMessage = "Some database error occured: Error message: " + ex0.Message
                                    + ", Caused by: " + cmd.CommandText;
                    return result;
                }
                return result;
            }
            else
            {
                return result;
            }
        }
        /// <summary>
        /// Method for getting a count of applicants for groups where user is leader
        /// If a user occurs more than once in group applications table, all entries are counted
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>count of applicants for groups where user is leader</returns>
        public int GetApplicantCountLeader(int userId)
        {
            int result = 0;

            if (userId == -1) { return result; }

            if (this.OpenConnection() == true)
            {
                MySqlTransaction mst = null;
                MySqlCommand cmd = null;

                try
                {
                    mst = this.connection.BeginTransaction();
                    cmd = new MySqlCommand();
                    cmd.Connection = this.connection;
                    cmd.Transaction = mst;

                    string count = "SELECT COUNT(*) FROM groupapplicants NATURAL JOIN "
                                   + "(SELECT groupId FROM users NATURAL JOIN groupmembers WHERE userId = @uid AND groupLeader = 1) AS g";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.CommandText = count;
                    cmd.Prepare();

                    result = Convert.ToInt32(cmd.ExecuteScalar());
                    this.CloseConnection();
                }
                catch (MySqlException ex0)
                {
                    this.CloseConnection();
                    ErrorMessage = "Some database error occured: Error message: " + ex0.Message
                                    + ", Caused by: " + cmd.CommandText;
                    return result;
                }
                return result;
            }
            else
            {
                return result;
            }
        }
    }
}