using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestApi.Model;

namespace RestApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private Context Context = new Context();

        [HttpGet]
        public List<SqlItem> ListItems()
        {
            List<SqlItem> Items = Context.SqlItems.ToList();
            
            foreach(SqlItem item in Items)
            {
                item.SqlShop = Context.SqlShops.Where(x => x.ShopID == item.ShopID).FirstOrDefault();
            }

            return Items;
        }

        [Route("{id}")]
        public SqlItem Get(int id)
        {
            SqlItem item = Context.SqlItems.Where(x => x.ItemID == id).FirstOrDefault();
            item.SqlShop = Context.SqlShops.Where(x => x.ShopID == item.ShopID).FirstOrDefault();

            return item;
        }

        [Route("listbyname/{name}")]
        public List<SqlItem> Get2(string name)
        {
            SqlShop shop = Context.SqlShops.Where(x => x.Name == name).OrderBy(x => x.ShopID).LastOrDefault();

            List<SqlItem> items = Context.SqlItems.Where(x => x.ShopID == shop.ShopID).ToList();
            foreach (SqlItem item in items)
            {
                item.SqlShop = shop;
            }

            return items;
        }

        [Route("listbyid/{id}")]
        public List<SqlItem> Get5(int id)
        {
            SqlShop shop = Context.SqlShops.Where(x => x.ShopID == id).FirstOrDefault();

            List<SqlItem> items = Context.SqlItems.Where(x => x.ShopID == shop.ShopID).ToList();
            foreach (SqlItem item in items)
            {
                item.SqlShop = shop;
            }

            return items;
        }

        [Route("listshops")]
        public List<SqlShop> GetShops()
        {
            List<SqlShop> Shops = Context.SqlShops.ToList();
            return Shops;
        }

        [Route("listshopsbyname/{name}")]
        public List<SqlShop> GetShopsByName(string name)
        {
            List<SqlShop> Shops = Context.SqlShops.Where(x => x.Name == name).ToList();
            return Shops;
        }

        [Route("addshop/{shopname}")]
        public SqlShop Add(string shopname)
        {
            SqlShop shop = new SqlShop()
            {
                Name = shopname,
                CreatedOn = DateTime.Now
            };

            Context.SqlShops.Add(shop);
            Context.SaveChanges();

            return shop;
        }

        [Route("additem/{shopname}/{itemname}/{quantity}")]
        public SqlItem Add2(string shopname, string itemname, double quantity)
        {
            SqlShop shop = Context.SqlShops.Where(x => x.Name.ToLower() == shopname.ToLower()).OrderBy(x => x.ShopID).LastOrDefault();

            SqlItem item = new SqlItem()
            {
                ShopID = shop.ShopID,
                Name = itemname,
                Quantity = quantity
            };

            Context.SqlItems.Add(item);
            Context.SaveChanges();

            return item;
        }

        [Route("removeitems/{id}")]
        public bool Remove(int id)
        {
            List<SqlItem> SqlItems = Context.SqlItems.Where(x => x.ShopID == id).ToList();

            foreach (SqlItem item in SqlItems)
            {
                Context.SqlItems.Remove(item);
                Context.SaveChanges();
            }

            SqlShop sqlShop = Context.SqlShops.Where(x => x.ShopID == id).FirstOrDefault();
            Context.SqlShops.Remove(sqlShop);
            Context.SaveChanges();

            return true;
        }
    }
}
