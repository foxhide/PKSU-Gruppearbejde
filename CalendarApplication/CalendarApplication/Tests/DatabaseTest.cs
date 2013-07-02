using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CalendarApplication.Database;
using CalendarApplication.Models.User;
using CalendarApplication.Models.Account;
using CalendarApplication.Models.Event;
using CalendarApplication.Models.EventType;
using CalendarApplication.Models.Group;
using CalendarApplication.Models.Maintenance;

namespace CalendarApplication.Tests
{
    [TestClass]
    public class DatabaseTest
    {
        /*
         * This test will only pass if the test_data mysql script
         * is run first.
         * This method tests the ExecuteQuery method using CustomQuery,
         * as the old ExecuteQuery method is deprecated.
         */
        [TestMethod]
        public void TestExecuteQuery()
        {
            MySqlConnect msc = new MySqlConnect();

            // Test ExecuteQuery with a single query
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT userName,email,admin FROM users WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)4 }
            };
            // Should return only one row, with user bill123, id=4,non-admin.

            DataTable dt = msc.ExecuteQuery(query);

            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual("bill123", dt.Rows[0]["userName"]);
            Assert.AreEqual("bill@kerbal.kb", dt.Rows[0]["email"]);
            Assert.AreEqual(false, dt.Rows[0]["admin"]);

            Assert.IsNull(msc.ErrorMessage);

            // Test ExecuteQuery with multiple queries
            CustomQuery[] queries = new CustomQuery[3];
            queries[0] = new CustomQuery
            {
                Cmd = "SELECT eventName,userId,eventTypeId,eventStart,eventEnd,visible,state"
                        + " FROM events WHERE eventId = @eid",
                ArgNames = new[] { "@eid" },
                Args = new[] { (object)1 }
            };
            queries[1] = new CustomQuery
            {
                Cmd = "SELECT fieldId FROM eventtypefields WHERE eventTypeId = @tid",
                ArgNames = new[] { "@tid" },
                Args = new[] { (object)2 }
            };
            queries[2] = new CustomQuery
            {
                Cmd = "SELECT * FROM table_2 WHERE eventId = @eid",
                ArgNames = new[] { "@eid" },
                Args = new[] { (object)1 }
            };

            DataSet ds = msc.ExecuteQuery(queries);

            Assert.IsNotNull(ds);
            Assert.AreEqual(ds.Tables.Count, 3);
            dt = ds.Tables[0];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(7, dt.Columns.Count);
            Assert.AreEqual(3, dt.Rows[0]["userId"]);
            Assert.AreEqual(2, dt.Rows[0]["eventTypeId"]);
            Assert.AreEqual("Muse", dt.Rows[0]["eventName"]);
            Assert.AreEqual(new DateTime(2013, 03, 19, 22, 0, 0), dt.Rows[0]["eventStart"]);
            Assert.AreEqual(new DateTime(2013, 03, 20, 2, 30, 0), dt.Rows[0]["eventEnd"]);
            Assert.AreEqual(true, dt.Rows[0]["visible"]);
            Assert.AreEqual(0, dt.Rows[0]["state"]);

            dt = ds.Tables[1];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(1, dt.Columns.Count);
            Assert.AreEqual(1, dt.Rows[0]["fieldId"]);

            dt = ds.Tables[2];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(2, dt.Columns.Count);
            Assert.AreEqual(1, dt.Rows[0]["eventId"]);
            Assert.AreEqual("muse", dt.Rows[0]["field_1"]);

            Assert.IsNull(msc.ErrorMessage);

            // Test a failure
            query = new CustomQuery
            {
                Cmd = "SELECT * FROM no_table WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)2 }
            };
            dt = msc.ExecuteQuery(query);

            Assert.IsNull(dt);
            // Check that the error message is set
            Assert.IsNotNull(msc.ErrorMessage);
        }

        /*
         * Tests the creation, edit, and deletion of users in the db.
         */
        [TestMethod]
        public void TestMySqlUser()
        {
            MySqlUser msu = new MySqlUser();

            // Try to make a user with no user name
            Register reg = new Register
            {
                RealName = "Newton",
                UserName = null,
                Email = "newton@gravity.com",
                Phone = "012345678",
                Password = "Apple"
            };

            int i = msu.CreateUser(reg);

            // Check for failure
            Assert.AreEqual(-1, i);
            Assert.IsNotNull(msu.ErrorMessage);
            msu.ErrorMessage = null;

            reg.UserName = "newton1337";

            i = msu.CreateUser(reg);

            Assert.IsTrue(i > 4); // bill123 from script has id=4
            Assert.IsNull(msu.ErrorMessage);

            // Update newtons Real Name and approval

            bool ok = msu.EditUser(i, "Einstein", "realName");
            Assert.IsTrue(ok);
            ok = msu.EditUser(i, false, "needsApproval");
            Assert.IsTrue(ok);

            // Check that changes are made in the db.
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM users WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)i }
            };

            DataTable dt = msu.ExecuteQuery(query);
            
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);

            Assert.AreEqual("Einstein", dt.Rows[0]["realName"]);
            Assert.AreEqual("newton1337", dt.Rows[0]["userName"]);
            Assert.AreEqual("012345678", dt.Rows[0]["phoneNum"]);
            Assert.AreEqual("newton@gravity.com", dt.Rows[0]["email"]);
            Assert.AreEqual(false, dt.Rows[0]["needsApproval"]);
            Assert.AreEqual(false, dt.Rows[0]["admin"]);
            Assert.AreEqual(true, dt.Rows[0]["active"]);

            // Now try to delete newton1337
            ok = msu.deleteUser(i);
            Assert.IsTrue(ok);
        }

        /*
         * Tests the creation, edit, and deletion of rooms in the db.
         */
        [TestMethod]
        public void TestMySqlRoom()
        {
            MySqlRoom msr = new MySqlRoom();

            // Try to create a null room -> fail
            int id = msr.CreateRoom(null);
            Assert.AreEqual(-1, id);
            Assert.IsNotNull(msr.ErrorMessage);
            msr.ErrorMessage = null;

            // Try to create a room
            id = msr.CreateRoom("MyTestRoom");
            Assert.IsTrue(id > 7); // 7 rooms from script
            Assert.IsNull(msr.ErrorMessage);

            // Check that new room is in db
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM rooms WHERE roomId = @rid",
                ArgNames = new[] { "@rid" }, Args = new[] { (object)id }
            };
            DataTable dt = msr.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);

            Assert.AreEqual("MyTestRoom", dt.Rows[0]["roomName"]);

            // Try to edit the room-name
            bool ok = msr.RenameRoom(id, "MyRenamedRoom");
            Assert.IsTrue(ok);
            Assert.IsNull(msr.ErrorMessage);

            // Check that update made it to db
            dt = msr.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);

            Assert.AreEqual("MyRenamedRoom", dt.Rows[0]["roomName"]);

            // Now, delete the room
            ok = msr.DeleteRoom(id);
            Assert.IsTrue(ok);
            Assert.IsNull(msr.ErrorMessage);
        }

        /*
         * Tests the creation and edit of groups in the db.
         * Remember to run the script before testing
         */
        [TestMethod]
        public void TestMySqlGroup()
        {
            MySqlGroup msg = new MySqlGroup();

            // Make group model
            GroupModel gm = new GroupModel
            {
                Name = "MyTestGroup",
                groupMembers = new List<SelectListItem>(),
                groupLeaders = new List<SelectListItem>(),
                canCreate = new List<SelectListItem>()
            };
            gm.groupMembers.Add(new SelectListItem { Value = "1", Text = "johan_the_man", Selected = true });
            gm.groupMembers.Add(new SelectListItem { Value = null, Text = "andreas_PKSU", Selected = true }); // Will give an error
            gm.groupMembers.Add(new SelectListItem { Value = "3", Text = "stephan_kerbal", Selected = false });
            gm.groupMembers.Add(new SelectListItem { Value = "4", Text = "bill123", Selected = true });

            // Try to create group, and fail.
            bool ok = msg.CreateGroup(gm);
            Assert.IsFalse(ok);
            Assert.IsNotNull(msg.ErrorMessage);
            msg.ErrorMessage = null;

            // Check that no group has been added (rollback successful)
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM groups WHERE groupName = @gname",
                ArgNames = new[] { "@gname" }, Args = new[] { (object)"MyTestGroup" }
            };
            DataTable dt = msg.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(0, dt.Rows.Count); // Check that it was not found

            // Correct error and try again.
            gm.groupMembers[1].Value = "2";
            ok = msg.CreateGroup(gm);
            Assert.IsTrue(ok);
            Assert.IsNull(msg.ErrorMessage);

            // Find it in the db
            dt = msg.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual("MyTestGroup", dt.Rows[0]["groupName"]);
            int id = (int)dt.Rows[0]["groupId"];
            Assert.IsTrue(id > 4); // 4 groups from db-test script

            //Add leaders & stuff
            gm.ID = id;

            gm.groupLeaders.Add(new SelectListItem { Value = "1", Text = "johan_the_man", Selected = false });
            gm.groupLeaders.Add(new SelectListItem { Value = "2", Text = "andreas_PKSU", Selected = true });
            gm.groupLeaders.Add(new SelectListItem { Value = "4", Text = "bill123", Selected = false });

            gm.canCreate.Add(new SelectListItem { Value = "1", Text = "johan_the_man", Selected = true });
            gm.canCreate.Add(new SelectListItem { Value = "2", Text = "andreas_PKSU", Selected = true });
            gm.canCreate.Add(new SelectListItem { Value = "4", Text = "bill123", Selected = false });

            ok = msg.EditGroup(gm);
            Assert.IsTrue(ok);
            Assert.IsNull(msg.ErrorMessage);

            // Test that members and leaders are added correctly
            CustomQuery queryMembers = new CustomQuery
            {
                Cmd = "SELECT * FROM groupmembers WHERE groupId = @gid",
                ArgNames = new[] { "@gid" }, Args = new[] { (object)id }
            };
            dt = msg.ExecuteQuery(queryMembers);
            Assert.IsNotNull(dt);
            Assert.AreEqual(3, dt.Rows.Count); // We inserted three members
            foreach (DataRow dr in dt.Rows)
            {
                int userId = (int)dr["userid"];
                Assert.IsTrue(userId == 1 || userId == 2 || userId == 4);
                switch (userId)
                {
                    case 1: Assert.AreEqual(true, dr["canCreate"]);
                        Assert.AreEqual(false, dr["groupLeader"]);
                        break;
                    case 2: Assert.AreEqual(true, dr["canCreate"]);
                        Assert.AreEqual(true, dr["groupLeader"]);
                        break;
                    case 4: Assert.AreEqual(false, dr["canCreate"]);
                        Assert.AreEqual(false, dr["groupLeader"]);
                        break;
                }
            }

            // Edit the group name, add a member (leader), remove a member, and check again.
            gm.Name = "MyOtherTestGroup";
            gm.groupMembers[3].Selected = false;
            gm.groupMembers[2].Selected = true;

            gm.groupLeaders.RemoveAt(2);
            gm.groupLeaders.Add(new SelectListItem { Value = "3", Text = "stephan_kerbal", Selected = true });

            gm.canCreate.RemoveAt(2);
            gm.canCreate.Add(new SelectListItem { Value = "3", Text = "stephan_kerbal", Selected = true });

            ok = msg.EditGroup(gm);
            Assert.IsTrue(ok);
            Assert.IsNull(msg.ErrorMessage);

            // Find it in the db
            query = new CustomQuery
            {
                Cmd = "SELECT * FROM groups WHERE groupId = @gid",
                ArgNames = new[] { "@gid" },
                Args = new[] { (object)id }
            };
            dt = msg.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual("MyOtherTestGroup", dt.Rows[0]["groupName"]);

            // Test that members are added correctly
            queryMembers = new CustomQuery
            {
                Cmd = "SELECT * FROM groupmembers WHERE groupId = @gid",
                ArgNames = new[] { "@gid" },
                Args = new[] { (object)id }
            };
            dt = msg.ExecuteQuery(queryMembers);
            Assert.IsNotNull(dt);
            Assert.AreEqual(3, dt.Rows.Count); // We inserted one and removed one
            foreach (DataRow dr in dt.Rows)
            {
                int userId = (int)dr["userid"];
                Assert.IsTrue(userId == 1 || userId == 2 || userId == 3);
                switch (userId)
                {
                    case 1: Assert.AreEqual(true, dr["canCreate"]);
                        Assert.AreEqual(false, dr["groupLeader"]);
                        break;
                    case 2: Assert.AreEqual(true, dr["canCreate"]);
                        Assert.AreEqual(true, dr["groupLeader"]);
                        break;
                    case 3: Assert.AreEqual(true, dr["canCreate"]);
                        Assert.AreEqual(true, dr["groupLeader"]);
                        break;
                }
            }

        }

        /*
         * Tests the creation and edit of eventtypes in the db.
         * Remember to run the script before testing
         * Remember to delete table_4 (or whatever the new table name is) afterwards
         */
        [TestMethod]
        public void TestMySqlEvent_Types()
        {
            MySqlEvent mse = new MySqlEvent();
            
            // Try to create a new event type
            EventTypeModel etm = new EventTypeModel
            {
                ID = -1,
                Name = "MyTestEventType",
                TypeSpecific = new List<FieldDataModel>()
            };
            etm.TypeSpecific.Add(new FieldDataModel
            {
                Name = null,
                Description = "Field 1 for testing",
                RequiredApprove = false,
                RequiredCreate = false,
                Datatype = Fieldtype.Bool
            });
            etm.TypeSpecific.Add(new FieldDataModel
            {
                Name = "TestField2",
                Description = "Field 2 for testing",
                RequiredApprove = true,
                RequiredCreate = false,
                Datatype = Fieldtype.Text,
                VarcharLength = 100
            });
            etm.TypeSpecific.Add(new FieldDataModel
            {
                Name = "TestField3",
                Description = "Field 3 for testing",
                RequiredApprove = false,
                RequiredCreate = false,
                Datatype = Fieldtype.User
            });
            etm.TypeSpecific.Add(new FieldDataModel
            {
                Name = "TestField4",
                Description = "Field 4 for testing",
                RequiredApprove = true,
                RequiredCreate = false,
                Datatype = Fieldtype.User
            });

            // Try and get an error
            bool ok = mse.CreateEventType(etm);
            Assert.IsFalse(ok);
            Assert.IsNotNull(mse.ErrorMessage);
            mse.ErrorMessage = null;

            // Check that nothing is added to the db.
            CustomQuery query = new CustomQuery
            {
                Cmd = "SELECT * FROM eventtypes WHERE eventTypeName = @etn",
                ArgNames = new[] { "@etn" }, Args = new[] { (object)"MyTestEventType" }
            };
            DataTable dt = mse.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.IsNull(mse.ErrorMessage);
            Assert.AreEqual(0, dt.Rows.Count);

            // Correct and try again
            etm.TypeSpecific[0].Name = "TestField1";
            ok = mse.CreateEventType(etm);
            Assert.IsTrue(ok);
            Assert.IsNull(mse.ErrorMessage);

            // Check that it is added to the db
            dt = mse.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);

            int id = (int)dt.Rows[0]["eventTypeId"];
            etm.ID = id;

            // Check, that the fields are added
            query.Cmd = "SELECT * FROM eventtypefields WHERE eventTypeId = @eid ORDER BY fieldId";
            query.ArgNames = new[] { "@eid" };
            query.Args = new[] { (object)id };
            dt = mse.ExecuteQuery(query);
            Assert.IsNotNull(dt);
            Assert.AreEqual(dt.Rows.Count, 4);

            for (int i = 0; i < 4; i++)
            {
                etm.TypeSpecific[i].ID = (int)dt.Rows[i]["fieldId"];
                Assert.AreEqual(etm.TypeSpecific[i].Name, dt.Rows[i]["fieldName"]);
                Assert.AreEqual(etm.TypeSpecific[i].Description, dt.Rows[i]["fieldDescription"]);
                Assert.AreEqual(etm.TypeSpecific[i].RequiredCreate, dt.Rows[i]["requiredCreation"]);
                Assert.AreEqual(etm.TypeSpecific[i].RequiredApprove, dt.Rows[i]["requiredApproval"]);
                Assert.AreEqual(etm.TypeSpecific[i].Datatype, (Fieldtype)dt.Rows[i]["fieldType"]);
                Assert.AreEqual(etm.TypeSpecific[i].VarcharLength, dt.Rows[i]["varCharLength"]);
            }

            // Check if table is set up correct
            CustomQuery queryTable = new CustomQuery { Cmd = "SELECT * FROM table_" + id };
            dt = mse.ExecuteQuery(queryTable);
            Assert.IsNotNull(dt);
            Assert.AreEqual(5, dt.Columns.Count);

            // Edit the event type and check again
            etm.Name = "I changed the name";
            etm.TypeSpecific[1].Description = "This is a new description";
            etm.TypeSpecific[2].RequiredApprove = true;
            etm.TypeSpecific[2].Description = "This is another new description";
            etm.TypeSpecific[3].ViewID = -1; // Remove it!

            // Edit the event type
            ok = mse.EditEventType(etm);
            Assert.IsTrue(ok);
            Assert.IsNull(mse.ErrorMessage);

            // Check that it is added to the db
            CustomQuery query1 = new CustomQuery
            {
                Cmd = "SELECT * FROM eventtypes WHERE eventTypeId = @eid",
                ArgNames = new[] { "@eid" },
                Args = new[] { (object)etm.ID }
            };
            dt = mse.ExecuteQuery(query1);
            Assert.IsNotNull(dt);
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(etm.Name, dt.Rows[0]["eventTypeName"]);

            // Check, that the fields are added
            dt = mse.ExecuteQuery(query);    // Same query as before
            Assert.IsNotNull(dt);
            Assert.AreEqual(dt.Rows.Count, 3);  // We removed a row

            for (int i = 0; i < 3; i++)
            {
                etm.TypeSpecific[i].ID = (int)dt.Rows[i]["fieldId"];
                Assert.AreEqual(etm.TypeSpecific[i].Name, dt.Rows[i]["fieldName"]);
                Assert.AreEqual(etm.TypeSpecific[i].Description, dt.Rows[i]["fieldDescription"]);
                Assert.AreEqual(etm.TypeSpecific[i].RequiredCreate, dt.Rows[i]["requiredCreation"]);
                Assert.AreEqual(etm.TypeSpecific[i].RequiredApprove, dt.Rows[i]["requiredApproval"]);
                Assert.AreEqual(etm.TypeSpecific[i].Datatype, (Fieldtype)dt.Rows[i]["fieldType"]);
                Assert.AreEqual(etm.TypeSpecific[i].VarcharLength, dt.Rows[i]["varCharLength"]);
            }

            dt = mse.ExecuteQuery(queryTable);
            Assert.IsNotNull(dt);
            Assert.AreEqual(4, dt.Columns.Count); //Again, a field was removed
        }

        /*
         * Tests the creation and edit of events in the db.
         * Remember to run the script before testing
         */
        [TestMethod]
        public void TestMySqlEvent_Events()
        {
            MySqlEvent mse = new MySqlEvent();

            // Try to create a new event
            EventEditModel eem = new EventEditModel
            {
                ID = -1,
                Name = "MyTestEvent",
                Visible = false,
                CreatorId = 3,
                State = 0,
                Start = new DateTime(2013,5,25,18,30,0),
                End = new DateTime(2013,5,26,4,0,0),
                RoomSelectList = new List<SelectListItem>(),
                TypeSpecifics = new List<FieldModel>(),
                Approved = false,
                SelectedEventType = "2", // Bad event type
                GroupEditorList = new List<SelectListItem>(),
                UserEditorList = new List<SelectListItem>(),
                GroupVisibleList = new List<SelectListItem>()
            };

            eem.TypeSpecifics.Add(new FieldModel
            {
                ID = 1,
                Name = "Band-name",
                Datatype = Fieldtype.Text,
                VarcharLength = 50,
                StringValue = "Killers"
            });

            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 1", Value = "1", Selected = true });
            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 2", Value = "2", Selected = false });
            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 3", Value = "3", Selected = false });
            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 4", Value = "4", Selected = true });
            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 5", Value = "5", Selected = true });
            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 6", Value = "6", Selected = false });
            eem.RoomSelectList.Add(new SelectListItem { Text = "Room 7", Value = "7", Selected = true });

            eem.GroupEditorList.Add(new SelectListItem { Text = "Pokerplayers", Value = "-1", Selected = true }); // Bad ID
            eem.GroupEditorList.Add(new SelectListItem { Text = "Bartenders", Value = "2", Selected = false });
            eem.GroupEditorList.Add(new SelectListItem { Text = "MG", Value = "3", Selected = false });
            eem.GroupEditorList.Add(new SelectListItem { Text = "L04", Value = "4", Selected = true });

            eem.UserEditorList.Add(new SelectListItem { Text = "Johan", Value = "1", Selected = false });
            eem.UserEditorList.Add(new SelectListItem { Text = "Andreas", Value = "2", Selected = false });
            eem.UserEditorList.Add(new SelectListItem { Text = "Stephan", Value = "3", Selected = false });
            eem.UserEditorList.Add(new SelectListItem { Text = "Bill", Value = "4", Selected = true });

            eem.GroupVisibleList.Add(new SelectListItem { Text = "Pokerplayers", Value = "1", Selected = false });
            eem.GroupVisibleList.Add(new SelectListItem { Text = "Bartenders", Value = "2", Selected = true });
            eem.GroupVisibleList.Add(new SelectListItem { Text = "MG", Value = "3", Selected = false });
            eem.GroupVisibleList.Add(new SelectListItem { Text = "L04", Value = "4", Selected = false });

            int id = mse.EditEvent(eem);
            Assert.AreEqual(-1,id);
            Assert.IsNotNull(mse.ErrorMessage);
            mse.ErrorMessage = null;

            // Check that changes were rolled back:
            CustomQuery[] queries = new CustomQuery[2];
            queries[0] = new CustomQuery { Cmd = "SELECT * FROM events WHERE eventName = 'MyTestEvent'" };
            queries[1] = new CustomQuery { Cmd = "SELECT * FROM table_2 WHERE field_1 = 'Killers'" };

            DataSet ds = mse.ExecuteQuery(queries);
            Assert.IsNotNull(ds);
            Assert.AreEqual(ds.Tables[0].Rows.Count, 0);
            Assert.AreEqual(ds.Tables[1].Rows.Count, 0);

            // Correct the mistake and try again
            eem.GroupEditorList[0].Value = "1";
            id = mse.EditEvent(eem);
            eem.ID = id;
            Assert.IsTrue(id > 7);
            Assert.IsNull(mse.ErrorMessage);

            queries = new CustomQuery[6];
            queries[0] = new CustomQuery { Cmd = "SELECT * FROM events WHERE eventId = " + id };
            queries[1] = new CustomQuery { Cmd = "SELECT * FROM table_2 WHERE eventId = " + id };
            queries[2] = new CustomQuery { Cmd = "SELECT * FROM eventroomsused WHERE eventId = " + id + " ORDER BY roomId" };
            queries[3] = new CustomQuery { Cmd = "SELECT * FROM eventeditorsgroups WHERE eventId = " + id + " ORDER BY groupId" };
            queries[4] = new CustomQuery { Cmd = "SELECT * FROM eventeditorsusers WHERE eventId = " + id + " ORDER BY userId" };
            queries[5] = new CustomQuery { Cmd = "SELECT * FROM eventvisibility WHERE eventId = " + id + " ORDER BY groupId" };
            ds = mse.ExecuteQuery(queries);
            Assert.IsNotNull(ds);
            DataTable dt = ds.Tables[0];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(eem.Name, dt.Rows[0]["eventName"]);
            Assert.AreEqual(Convert.ToInt32(eem.SelectedEventType), dt.Rows[0]["eventTypeId"]);
            Assert.AreEqual(eem.Visible, dt.Rows[0]["visible"]);
            Assert.AreEqual(eem.State, dt.Rows[0]["state"]);
            Assert.AreEqual(eem.Start, dt.Rows[0]["eventStart"]);
            Assert.AreEqual(eem.End, dt.Rows[0]["eventEnd"]);
            Assert.AreEqual(eem.CreatorId, dt.Rows[0]["userId"]);

            // Check specefics table
            dt = ds.Tables[1];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(eem.TypeSpecifics[0].StringValue, dt.Rows[0]["field_1"]);

            // Check rooms
            dt = ds.Tables[2];
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["roomId"]);
            Assert.AreEqual(4, dt.Rows[1]["roomId"]);
            Assert.AreEqual(5, dt.Rows[2]["roomId"]);
            Assert.AreEqual(7, dt.Rows[3]["roomId"]);

            // Check editor groups
            dt = ds.Tables[3];
            Assert.AreEqual(2, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0]["groupId"]);
            Assert.AreEqual(4, dt.Rows[1]["groupId"]);

            // Check editor users
            dt = ds.Tables[4];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(4, dt.Rows[0]["userId"]);

            // Check visible groups
            dt = ds.Tables[5];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(2, dt.Rows[0]["groupId"]);

            // Now, edit the group:
            //  Change the band name, set the event to visible (all visible groups should be deleted),
            //  set the ending time to two hour later, remove room 1, and remove editor group with id=1.
            eem.TypeSpecifics[0].StringValue = "The Doors";
            eem.Visible = true;
            eem.End.AddHours(2);
            eem.RoomSelectList[0].Selected = false;
            eem.GroupEditorList[0].Selected = false;
            id = mse.EditEvent(eem);

            // Do the checks again.
            Assert.AreEqual(eem.ID, id);
            Assert.IsNull(mse.ErrorMessage);

            ds = mse.ExecuteQuery(queries); // Same queries as before
            Assert.IsNotNull(ds);
            dt = ds.Tables[0];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(eem.Name, dt.Rows[0]["eventName"]);
            Assert.AreEqual(Convert.ToInt32(eem.SelectedEventType), dt.Rows[0]["eventTypeId"]);
            Assert.AreEqual(eem.Visible, dt.Rows[0]["visible"]);  // Changed
            Assert.AreEqual(eem.State, dt.Rows[0]["state"]);
            Assert.AreEqual(eem.Start, dt.Rows[0]["eventStart"]);
            Assert.AreEqual(eem.End, dt.Rows[0]["eventEnd"]);     // Changed
            Assert.AreEqual(eem.CreatorId, dt.Rows[0]["userId"]);

            // Check specefics table
            dt = ds.Tables[1];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(eem.TypeSpecifics[0].StringValue, dt.Rows[0]["field_1"]);  // Changed

            // Check rooms
            dt = ds.Tables[2];
            Assert.AreEqual(3, dt.Rows.Count);
            Assert.AreEqual(4, dt.Rows[0]["roomId"]);
            Assert.AreEqual(5, dt.Rows[1]["roomId"]);
            Assert.AreEqual(7, dt.Rows[2]["roomId"]);

            // Check editor groups
            dt = ds.Tables[3];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(4, dt.Rows[0]["groupId"]);

            // Check editor users
            dt = ds.Tables[4];
            Assert.AreEqual(1, dt.Rows.Count);
            Assert.AreEqual(4, dt.Rows[0]["userId"]);

            // Check visible groups
            dt = ds.Tables[5];
            Assert.AreEqual(0, dt.Rows.Count);
        }
    }
}