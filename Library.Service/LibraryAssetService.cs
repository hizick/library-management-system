﻿using System.Collections.Generic;
using System.Linq;
using Library.Data;
using Library.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Service
{
    public class LibraryAssetService : ILibraryAssetService
    {
        private readonly LibraryDbContext _context;

        public LibraryAssetService(LibraryDbContext context)
        {
            _context = context;
        }

        public void Add(LibraryAsset newAsset)
        {
            _context.Add(newAsset);
            _context.SaveChanges();
        }

        public LibraryAsset Get(int id)
        {
            return _context.LibraryAssets
                .Include(a => a.Status)
                .Include(a => a.Location)
                .FirstOrDefault(a => a.Id == id);
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
            return _context.LibraryAssets
                .Include(a => a.Status)
                .Include(a => a.Location);
        }

        public string GetAuthorOrDirector(int id)
        {
            var isBook = _context.LibraryAssets
                .OfType<Book>().Any(a => a.Id == id);

            return isBook
                ? GetAuthor(id)
                : GetDirector(id);
        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return _context.LibraryAssets
                .FirstOrDefault(a => a.Id == id)
                ?.Location;
        }

        public string GetDeweyIndex(int id)
        {
            if (_context.Books.Any(a => a.Id == id))
                return _context.Books
                    .FirstOrDefault(a => a.Id == id)
                    ?.DeweyIndex;

            return "";
        }

        public string GetIsbn(int id)
        {
            if (GetType(id) != "Book") return "N/A";
            var book = (Book) Get(id);
            return book.ISBN;

        }

        public LibraryCard GetLibraryCardByAssetId(int id)
        {
            return _context.LibraryCards
                .FirstOrDefault(c => c.Checkouts
                    .Select(a => a.LibraryAsset)
                    .Select(v => v.Id).Contains(id));
        }

        // cannot access discriminator value directly without
        // building a raw sql query as far as I'm aware,
        // so we should either change mapping strategy, or 
        // perhaps we don't need to implement inheritance
        public string GetTitle(int id)
        {
            return _context.LibraryAssets.FirstOrDefault(a => a.Id == id)?.Title;
        }

        public string GetType(int id)
        {
            // you should be aware that the discriminator column is used internally 
            // by Code First and you cannnot read/write its values from an inheritance 
            // mapping standpoint.
            // Hack, not scalable - there is a better way.

            var books = _context.LibraryAssets
                .OfType<Book>().Where(a => a.Id == id);

            return books.Any() ? "Book" : "Video";
        }

        private string GetAuthor(int id)
        {
            var book = (Book) Get(id);
            return book.Author;
        }

        private string GetDirector(int id)
        {
            var video = (Video) Get(id);
            return video.Director;
        }
    }
}