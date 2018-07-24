using System;

// ADD THIS PART TO YOUR CODE
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;


namespace DocumentDBGettingStarted
{
    class Program
    {
        private const string EndopointUri = "https://localhost:8081";
        private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private DocumentClient client;
        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.GetStartedDemo().Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        private async Task GetStartedDemo()
        {
            this.client = new DocumentClient(new Uri(EndopointUri), PrimaryKey);
            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = "FamilyDB" });
            await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("FamilyDB"), new DocumentCollection { Id = "FamilyCollection" });

            Family andersenFamily = new Family

            {

                Id = "Andersen.1",

                LastName = "Andersen",

                Parents = new Parent[]

                {

                    new Parent { FirstName = "Thomas" },

                    new Parent { FirstName = "Mary Kay" }

                },

                Children = new Child[]

                {

                    new Child

                    {

                        FirstName = "Henriette Thaulow",

                        Gender = "female",

                        Grade = 5,

                        Pets = new Pet[]

                        {

                            new Pet { GivenName = "Fluffy" }

                        }

                    }

                },

                District = "WA5",

                Address = new Address { State = "WA", Country = "King", City = "Seattle" },

                IsRegistered = true

            };
            await this.CreateFamilyDocumentIfNotExists("FamilyDB", "FamilyCollection", andersenFamily);
            Family andersonFamilyDocument = await this.client.ReadDocumentAsync<Family>(UriFactory.CreateDocumentUri("FamilyDB", "FamilyCollection", andersenFamily.Id));

            Console.WriteLine("\nRead family {0}", andersonFamilyDocument.Id);

            this.WriteToConsoleAndPromptToContinue("{0}", andersonFamilyDocument);


            Family wakefieldFamily = new Family

            {

                Id = "Wakefield.7",

                LastName = "Wakefield",

                Parents = new Parent[]

                {

                    new Parent { FamilyName = "Wakefield", FirstName = "Robin" },

                    new Parent { FamilyName = "Miller", FirstName = "Ben" }

                },

                Children = new Child[]

                {

                    new Child

                    {

                        FamilyName = "Merriam",

                        FirstName = "Jesse",

                        Gender = "female",

                        Grade = 8,

                        Pets = new Pet[]

                        {

                            new Pet { GivenName = "Goofy" },

                            new Pet { GivenName = "Shadow" }

                        }

                    },

                    new Child

                    {

                        FamilyName = "Miller",

                        FirstName = "Lisa",

                        Gender = "female",

                        Grade = 1

                    }

                },

                District = "NY23",

                Address = new Address { State = "NY", Country = "Manhattan", City = "NY" },

                IsRegistered = false

            };



            await this.CreateFamilyDocumentIfNotExists("FamilyDB", "FamilyCollection", wakefieldFamily);



            Family wakefieldFamilyDocument = await this.client.ReadDocumentAsync<Family>(UriFactory.CreateDocumentUri("FamilyDB", "FamilyCollection", wakefieldFamily.Id));



            Console.WriteLine("\nRead family {0}", wakefieldFamilyDocument.Id);



            this.WriteToConsoleAndPromptToContinue("{0}", wakefieldFamilyDocument);



            this.ExecuteSimpleQuery("FamilyDB", "FamilyCollection");



            // Clean up/delete the database and client

            await this.client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("FamilyDB"));
        }
        private async Task CreateFamilyDocumentIfNotExists(string databaseName, string collectionName, Family family)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, family.Id));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), family);
                    this.WriteToConsoleAndPromptToContinue("\nCreated Family {0}", family.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ExecuteSimpleQuery(string databaseName,string collectionName)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            IQueryable<Family> familyQuery = this.client.CreateDocumentQuery<Family>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                .Where(f => f.LastName == "Andersen");
            

            Console.WriteLine("\nRunning LINQ query");
            foreach (Family family in familyQuery)
            {
                Console.WriteLine("\nRead {0}", family);
            }

            IQueryable<Family> familyQueryInSql = this.client.CreateDocumentQuery<Family>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                "SELECT * FROM Family WHERE Family.LastName='Andersen' ",
                queryOptions);

        }

        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        public class Family
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string LastName { get; set; }
            public string District { get; set; }
            public Parent[] Parents { get; set; }
            public Child[] Children { get; set; }
            public Address Address { get; set; }
            public bool IsRegistered { get; set; }
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }

        }
        public class Parent
        {
            public string FamilyName { get; set; }
            public string FirstName { get; set; }
        }

        public class Child
        {
            public string FamilyName { get; set; }
            public string FirstName { get; set; }
            public string Gender { get; set; }
            public int Grade { get; set; }
            public Pet[] Pets { get; set; }
        }

        public class Pet
        {
            public string GivenName { get; set; }
        }
        public class Address
        {
            public string State { get; set; }
            public string Country { get; set; }
            public string City { get; set; }
        }
    }
}
