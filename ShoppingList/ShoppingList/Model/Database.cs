using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace ShoppingList.Model
{
    public class Database
    {
        readonly SQLiteAsyncConnection _database;

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Item>().Wait();
            _database.CreateTableAsync<Shop>().Wait();
            _database.CreateTableAsync<Setting>().Wait();
        }

        public Task<List<Item>> GetItemsAsync()
        {
            return _database.Table<Item>().ToListAsync();
        }
        public Task<List<Item>> GetItemsByShopAsync(int id)
        {
            return _database.Table<Item>().Where(x => x.ShopID == id).ToListAsync();
        }
        public Task<List<Shop>> GetShopsAsync()
        {
            return _database.Table<Shop>().ToListAsync();
        }
        public Task<Item> GetItemAsync(Item item)
        {
            return _database.Table<Item>().Where(x => x.ItemID == item.ItemID).FirstOrDefaultAsync();
        }
        public Task<Shop> GetShopAsync(Shop shop)
        {
            return _database.Table<Shop>().Where(x => x.ShopID == shop.ShopID).FirstOrDefaultAsync();
        }

        public Task<Shop> GetShopByID(int id)
        {
            return _database.Table<Shop>().Where(x => x.ShopID == id).FirstOrDefaultAsync();
        }

        public Task<Shop> GetShopByName(string Name)
        {
            return _database.Table<Shop>().Where(x => x.Name == Name).FirstOrDefaultAsync();
        }

        public Task SaveItemAsync(Item item)
        {
            return _database.InsertAsync(item);
        }
        public Task SaveShopAsync(Shop shop)
        {
            return _database.InsertAsync(shop);
        }

        public Task UpdateItemAsync(Item item)
        {
            return _database.UpdateAsync(item);
        }
        public Task UpdateShopAsync(Shop shop)
        {
            return _database.UpdateAsync(shop);
        }
        public Task InsertItemAsync(Item item)
        {
            return _database.InsertOrReplaceAsync(item);
        }
        public Task InsertShopAsync(Shop shop)
        {
            return _database.InsertOrReplaceAsync(shop);
        }
        public Task DeleteItemAsync(Item item)
        {
            return _database.DeleteAsync(item);
        }
        public Task DeleteShopAsync(Shop shop)
        {
            return _database.DeleteAsync(shop);
        }

        public Task<Setting> GetSettingsAsync()
        {
            return _database.Table<Setting>().FirstOrDefaultAsync();
        }

        public Task InsertSettingAsync(Setting setting)
        {
            return _database.InsertAsync(setting);
        }

        public Task UpdateSettingAsync(Setting setting)
        {
            return _database.UpdateAsync(setting);
        }
    }
}
