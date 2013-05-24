using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CalendarApplication.Database;

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
                Cmd = "SELECT userName,email,admin FROM pksudb.users WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)4 }
            };
            // Should return only one row, with user bill123, id=4,non-admin.

            DataTable dt = msc.ExecuteQuery(query);

            Assert.IsNotNull(dt);
            Assert.AreEqual(dt.Rows.Count, 1);
            Assert.AreEqual(dt.Rows[0]["userName"], "bill123");
            Assert.AreEqual(dt.Rows[0]["email"], "bill@kerbal.kb");
            Assert.AreEqual(dt.Rows[0]["admin"], false);

            Assert.IsNull(msc.ErrorMessage);

            // Test ExecuteQuery with multiple queries
            CustomQuery[] queries = new CustomQuery[3];
            queries[0] = new CustomQuery
            {
                Cmd = "SELECT eventName,userId,eventTypeId,eventStart,eventEnd,visible,state"
                        + " FROM pksudb.events WHERE eventId = @eid",
                ArgNames = new[] { "@eid" },
                Args = new[] { (object)1 }
            };
            queries[1] = new CustomQuery
            {
                Cmd = "SELECT fieldId FROM pksudb.eventtypefields WHERE eventTypeId = @tid",
                ArgNames = new[] { "@tid" },
                Args = new[] { (object)2 }
            };
            queries[2] = new CustomQuery
            {
                Cmd = "SELECT * FROM pksudb.table_2 WHERE eventId = @eid",
                ArgNames = new[] { "@eid" },
                Args = new[] { (object)1 }
            };

            DataSet ds = msc.ExecuteQuery(queries);

            Assert.IsNotNull(ds);
            Assert.AreEqual(ds.Tables.Count, 3);
            dt = ds.Tables[0];
            Assert.AreEqual(dt.Rows.Count, 1);
            Assert.AreEqual(dt.Columns.Count, 7);
            Assert.AreEqual(dt.Rows[0]["userId"],3);
            Assert.AreEqual(dt.Rows[0]["eventTypeId"],2);
            Assert.AreEqual(dt.Rows[0]["eventName"],"Muse");
            Assert.AreEqual(dt.Rows[0]["eventStart"], new DateTime(2013, 03, 19, 22, 0, 0));
            Assert.AreEqual(dt.Rows[0]["eventEnd"],new DateTime(2013,03,20,2,30,0));
            Assert.AreEqual(dt.Rows[0]["visible"],true);
            Assert.AreEqual(dt.Rows[0]["state"],0);

            dt = ds.Tables[1];
            Assert.AreEqual(dt.Rows.Count, 1);
            Assert.AreEqual(dt.Columns.Count, 1);
            Assert.AreEqual(dt.Rows[0]["fieldId"], 1);

            dt = ds.Tables[2];
            Assert.AreEqual(dt.Rows.Count, 1);
            Assert.AreEqual(dt.Columns.Count, 2);
            Assert.AreEqual(dt.Rows[0]["eventId"], 1);
            Assert.AreEqual(dt.Rows[0]["field_1"], "muse");

            Assert.IsNull(msc.ErrorMessage);

            // Test a failure
            query = new CustomQuery
            {
                Cmd = "SELECT * FROM pksudb.no_table WHERE userId = @uid",
                ArgNames = new[] { "@uid" },
                Args = new[] { (object)2 }
            };
            dt = msc.ExecuteQuery(query);

            Assert.IsNull(dt);
            // Check that the error message is set
            Assert.IsNotNull(msc.ErrorMessage);
        }
    }
}