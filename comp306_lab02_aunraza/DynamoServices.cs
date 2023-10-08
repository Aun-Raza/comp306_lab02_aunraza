using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace comp306_lab02_aunraza
{
    public static class DynamoServices
    {
        public readonly static string USER_TABLE_NAME = "User";
        public readonly static string BOOKSELF_TABLE_NAME = "Bookshelf";
        private readonly static IAmazonDynamoDB _dynamoClient;

        static DynamoServices()
        {
            _dynamoClient = new AmazonDynamoDBClient(Amazon.RegionEndpoint.CACentral1);
        }

        public static async Task<Document> FindUserByUsernameAndPassword(string username, string password)
        {
            try
            {
                Table userTable = LoadTable(_dynamoClient, USER_TABLE_NAME);
                var filter = new QueryFilter("Username", QueryOperator.Equal, username);
                filter.AddCondition("Password", QueryOperator.Equal, password);

                Search search = userTable.Query(filter);
                List<Document> documentList = new List<Document>();

                do
                {
                    documentList.AddRange(await search.GetNextSetAsync());
                }
                while (!search.IsDone);

                return documentList.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static async Task<List<Document>> FindBooksWithUsername(string username)
        {
            try
            {
                Table bookselfTable = LoadTable(_dynamoClient, BOOKSELF_TABLE_NAME);
                var scanFilter = new ScanFilter();
                scanFilter.AddCondition("Username", ScanOperator.Equal, username);

                Search search = bookselfTable.Scan(scanFilter);
                List<Document> documentList = new List<Document>();

                do
                {
                    documentList.AddRange(await search.GetNextSetAsync());
                }
                while (!search.IsDone);

                return documentList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static T ConvertDocumentTo<T>(Document document) where T : new()
        {
            var obj = new T();

            foreach (var attribute in document.GetAttributeNames())
            {
                var propertyInfo = typeof(T).GetProperty(attribute);

                if (propertyInfo != null)
                {
                    var value = document[attribute];

                    if (value is Primitive)
                    {
                        var stringValue = value.AsPrimitive().Value.ToString();
                        propertyInfo.SetValue(obj, Convert.ChangeType(stringValue, propertyInfo.PropertyType));
                    }
                    else if (value is PrimitiveList)
                    {
                        // This assumes the property is a List<string> or string[]
                        var listValue = (from primitive in value.AsPrimitiveList().Entries
                                         select primitive.Value.ToString()).ToList();

                        if (propertyInfo.PropertyType == typeof(string[]))
                        {
                            propertyInfo.SetValue(obj, listValue.ToArray());
                        }
                        else
                        {
                            propertyInfo.SetValue(obj, listValue);
                        }
                    }
                }
            }

            return obj;
        }

        public static async Task<bool> UpdateBookMark(string title, int pageNumber)
        {
            try
            {
                Table bookselfTable = LoadTable(_dynamoClient, BOOKSELF_TABLE_NAME);
                string dateTime = DateTime.Now.ToString("yyyy-MM-ddHH:mm:ss");
                var bookself = new Document
                {
                    ["Title"] = title,
                    ["BookMarkTime"] = dateTime,
                    ["BookMarkPage"] = pageNumber,
                };

                Document updatedBook = await bookselfTable.UpdateItemAsync(bookself);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private static Table LoadTable(IAmazonDynamoDB client, string tableName)
        {
            Table productCatalog = Table.LoadTable(client, tableName);
            return productCatalog;
        }


    }
}
