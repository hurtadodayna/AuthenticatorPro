﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Albireo.Base32;
using Javax.Xml.Xpath;
using OtpSharp;
using ProAuth.Data;
using SQLite;

namespace ProAuth.Utilities
{
    internal class CategorySource
    {
        public List<Category> Categories { get; private set; }
        public Task UpdateTask { get; }

        private readonly SQLiteAsyncConnection _connection;

        public CategorySource(SQLiteAsyncConnection connection)
        {
            Categories = new List<Category>();
            _connection = connection;

            UpdateTask = Update();
        }

        public async Task Update()
        {
            Categories.Clear();
            Categories = 
                await _connection.QueryAsync<Category>("SELECT * FROM category ORDER BY ranking ASC");
        }

        public bool IsDuplicate(Category category)
        {
            foreach(Category iterator in Categories)
            {
                if(category.Id == iterator.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task Delete(int position)
        {
            Category category = Categories[position];
            await _connection.DeleteAsync(category);
            Categories.RemoveAt(position);

            object[] args = {category.Id};
            _connection.ExecuteAsync("DELETE FROM authenticatorcategory WHERE categoryId = ?", args);
        }

        public void Rename(int position, string name)
        {
            Category old = Categories[position];
            Category replacement = new Category(name);

            Categories.RemoveAt(position);
            Categories.Add(replacement);

            _connection.DeleteAsync(old);
            _connection.InsertAsync(replacement);

            object[] args = {replacement.Id, old.Id};
            _connection.QueryAsync<AuthenticatorCategory>(
                "UPDATE authenticatorcategory SET categoryId = ? WHERE categoryId = ?", args);
        }

        public async void Move(int oldPosition, int newPosition)
        {
            Category old = Categories[newPosition];
            Categories[newPosition] = Categories[oldPosition];
            Categories[oldPosition] = old;

            if(oldPosition > newPosition)
            {
                for(int i = newPosition; i < Categories.Count; ++i)
                {
                    Category cat = Categories[i];
                    cat.Ranking++;
                    _connection.UpdateAsync(cat);
                }
            }
            else
            {
                for(int i = oldPosition; i < newPosition; ++i)
                {
                    Category cat = Categories[i]; 
                    cat.Ranking--;
                    _connection.UpdateAsync(cat);
                }
            }

            Category temp = Categories[newPosition]; 
            temp.Ranking = newPosition;
            _connection.UpdateAsync(temp);
        }

        public int Count()
        {
            return Categories.Count;
        }
    }
}