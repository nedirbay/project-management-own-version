using System;
using AutoFixture;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectManager.API.Data;

namespace Dinfo.Test.helpers
{
    public class TestBase : IDisposable
    {
        protected IFixture Fixture { get; }
        protected ApplicationDbContext Context { get; }
        private readonly IDbContextTransaction _transaction;

        public TestBase()
        {
            Context = DatabaseHelper.GetDbContext();
            Context.Database.EnsureCreated();
            DbInitializer.SeedAsync(Context).GetAwaiter().GetResult();

            _transaction = Context.Database.BeginTransaction();
            Fixture = new Fixture();
        }

        public void Dispose()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            Context.Dispose();
        }
    }
}
