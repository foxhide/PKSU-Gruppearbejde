﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

using CalendarApplication.Models.Group;

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
        /// Edit an existing group -WIP
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
                    for (int i = 0; i < groupmodel.groupMembers.Count; i++)
                    {
                        if (groupmodel.groupMembers[i].Selected)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO groupmembers (groupId, userId, groupLeader, canCreate) VALUES (@groupId, @userId, @groupLeader, @canCreate)";
                            cmd.Parameters.AddWithValue("@groupId", groupmodel.ID);
                            cmd.Parameters.AddWithValue("@userId", groupmodel.groupMembers[i].Value);
                            cmd.Parameters.AddWithValue("@groupLeader", 0);
                            cmd.Parameters.AddWithValue("@canCreate", 0);
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    for (int i = 0; i < groupmodel.groupLeaders.Count; i++)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE groupmembers SET groupLeader = @groupLeader, canCreate = @canCreate WHERE userId = @userId";
                        cmd.Parameters.AddWithValue("@groupLeader", groupmodel.groupLeaders[i].Selected);
                        cmd.Parameters.AddWithValue("@canCreate", groupmodel.groupLeaders[i].Selected || groupmodel.canCreate[i].Selected);
                        cmd.Parameters.AddWithValue("@userId", groupmodel.groupLeaders[i].Value);
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
    }
}