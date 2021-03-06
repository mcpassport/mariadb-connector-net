// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; version 3 of the License.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
// for more details.
//
// You should have received a copy of the GNU Lesser General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Data;
using System.IO;
using NUnit.Framework;

namespace MariaDB.Data.MySqlClient.Tests
{
    [TestFixture]
    public class BulkLoading : BaseTest
    {
        [Test]
        public void BulkLoadSimple()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 200; i++)
                sw.WriteLine(i + "\t'Test'");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            int count = loader.Load();
            Assert.AreEqual(200, count);
        }

        [Test]
        public void BulkLoadReadOnlyFile()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 200; i++)
                sw.WriteLine(i + "\t'Test'");
            sw.Flush();
            sw.Dispose();

            FileInfo fi = new FileInfo(path);
            FileAttributes oldAttr = fi.Attributes;
            fi.Attributes = fi.Attributes | FileAttributes.ReadOnly;
            try
            {
                MySqlBulkLoader loader = new MySqlBulkLoader(conn);
                loader.TableName = "Test";
                loader.FileName = path;
                loader.Timeout = 0;
                int count = loader.Load();
                Assert.AreEqual(200, count);
            }
            finally
            {
                fi.Attributes = oldAttr;
                fi.Delete();
            }
        }

        [Test]
        public void BulkLoadSimple2()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 200; i++)
                sw.Write(i + ",'Test' xxx");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            loader.LineTerminator = "xxx";
            int count = loader.Load();
            Assert.AreEqual(200, count);

            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Test", conn);
            Assert.AreEqual(200, cmd.ExecuteScalar());
        }

        [Test]
        public void BulkLoadSimple3()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 200; i++)
                sw.Write(i + ",'Test' xxx");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            loader.LineTerminator = "xxx";
            loader.NumberOfLinesToSkip = 50;
            int count = loader.Load();
            Assert.AreEqual(150, count);

            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Test", conn);
            Assert.AreEqual(150, cmd.ExecuteScalar());
        }

        [Test]
        public void BulkLoadSimple4()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 100; i++)
                sw.Write("aaa" + i + ",'Test' xxx");
            for (int i = 100; i < 200; i++)
                sw.Write("bbb" + i + ",'Test' xxx");
            for (int i = 200; i < 300; i++)
                sw.Write("aaa" + i + ",'Test' xxx");
            for (int i = 300; i < 400; i++)
                sw.Write("bbb" + i + ",'Test' xxx");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            loader.LineTerminator = "xxx";
            loader.LinePrefix = "bbb";
            int count = loader.Load();
            Assert.AreEqual(200, count);

            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Test", conn);
            Assert.AreEqual(200, cmd.ExecuteScalar());
        }

        [Test]
        public void BulkLoadFieldQuoting()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), name2 VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 200; i++)
                sw.WriteLine(i + "\t`col1`\tcol2");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldQuotationCharacter = '`';
            loader.FieldQuotationOptional = true;
            int count = loader.Load();
            Assert.AreEqual(200, count);
        }

        [Test]
        public void BulkLoadEscaping()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), name2 VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 200; i++)
                sw.WriteLine(i + ",col1\tstill col1,col2");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.EscapeCharacter = '\t';
            loader.FieldTerminator = ",";
            int count = loader.Load();
            Assert.AreEqual(200, count);
        }

        [Test]
        public void BulkLoadConflictOptionReplace()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 20; i++)
                sw.WriteLine(i + ",col1");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            int count = loader.Load();
            Assert.AreEqual(20, count);

            path = Path.GetTempFileName();
            sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 20; i++)
                sw.WriteLine(i + ",col2");
            sw.Flush();
            sw.Dispose();

            loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            loader.ConflictOption = MySqlBulkLoaderConflictOption.Replace;
            loader.Load();
        }

        [Test]
        public void BulkLoadConflictOptionIgnore()
        {
            execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 20; i++)
                sw.WriteLine(i + ",col1");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            int count = loader.Load();
            Assert.AreEqual(20, count);

            path = Path.GetTempFileName();
            sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 20; i++)
                sw.WriteLine(i + ",col2");
            sw.Flush();
            sw.Dispose();

            loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            loader.ConflictOption = MySqlBulkLoaderConflictOption.Ignore;
            loader.Load();
        }

        [Test]
        public void BulkLoadColumnOrder()
        {
            execSQL(@"CREATE TABLE Test (id INT NOT NULL, n1 VARCHAR(250), n2 VARCHAR(250),
                        n3 VARCHAR(250), PRIMARY KEY(id))");

            // first create the external file
            string path = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(File.OpenWrite(path));
            for (int i = 0; i < 20; i++)
                sw.WriteLine(i + ",col3,col2,col1");
            sw.Flush();
            sw.Dispose();

            MySqlBulkLoader loader = new MySqlBulkLoader(conn);
            loader.TableName = "Test";
            loader.FileName = path;
            loader.Timeout = 0;
            loader.FieldTerminator = ",";
            loader.LineTerminator = Environment.NewLine;
            loader.Columns.Add("id");
            loader.Columns.Add("n3");
            loader.Columns.Add("n2");
            loader.Columns.Add("n1");
            int count = loader.Load();
            Assert.AreEqual(20, count);
        }
    }
}