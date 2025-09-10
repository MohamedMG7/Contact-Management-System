using Contact_Management_system.DbHelper;
using Contact_Management_system.Dtos;
using Contact_Management_system.Managers;
using Contact_Management_system.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace Contact_Management_system.Tests.Managers
{
    public class ContactManagerTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"ContactsDb_{Guid.NewGuid()}")
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void AddContact_Should_Succeed_And_Persist()
        {
            using var db = CreateInMemoryContext();
            db.ApplicationUsers.Add(new ApplicationUser { Id = 1, Email = "user@example.com", Password = "hash" });
            db.SaveChanges();

            var sut = new ContactManager(db);
            var dto = new addContactDto("Sara", "Yousef", "+201111111111", new DateOnly(2001, 10, 12), "sara@example.com");

            var result = sut.AddContact(dto, userId: 1);

            result.success.Should().BeTrue();
            result.message.Should().Be("Contact added.");

            var saved = db.Contacts.Single(c => c.ApplicationUserId == 1 && c.PhoneNumber == "+201111111111");
            saved.FirstName.Should().Be("Sara");
            saved.LastName.Should().Be("Yousef");
            saved.EmailAddress.Should().Be("sara@example.com");
            saved.BirthDate.Should().Be(new DateOnly(2001, 10, 12));
        }

        [Fact]
        public void AddContact_Should_Fail_On_DuplicatePhone()
        {
            using var db = CreateInMemoryContext();
            db.ApplicationUsers.Add(new ApplicationUser { Id = 7, Email = "u@e.com", Password = "x" });
            db.Contacts.Add(new Contact
            {
                ApplicationUserId = 7,
                FirstName = "Existing",
                LastName = "Contact",
                PhoneNumber = "+201111111111",
                EmailAddress = "existing@example.com",
                BirthDate = new DateOnly(1990, 1, 1)
            });
            db.SaveChanges();

            var sut = new ContactManager(db);
            var dto = new addContactDto("New", "Person", "+201111111111", new DateOnly(2000, 5, 5), "new@example.com");

            var result = sut.AddContact(dto, userId: 7);

            result.success.Should().BeFalse();
            result.message.Should().Be("Contact with same email or phone already exists for this user.");
        }

        [Fact]
        public void GetUserAddressBook_Should_Return_Paged_Items()
        {
            using var db = CreateInMemoryContext();
            db.ApplicationUsers.Add(new ApplicationUser { Id = 1, Email = "u@e.com", Password = "x" });
            db.Contacts.AddRange(
                new Contact { ApplicationUserId = 1, FirstName = "Ali", LastName = "Khaled", PhoneNumber = "1", EmailAddress = "a@a.com", BirthDate = new DateOnly(1999, 3, 25) },
                new Contact { ApplicationUserId = 1, FirstName = "Sara", LastName = "Yousef", PhoneNumber = "2", EmailAddress = "s@s.com", BirthDate = new DateOnly(2001, 10, 12) },
                new Contact { ApplicationUserId = 1, FirstName = "Bana", LastName = "Zaki", PhoneNumber = "3", EmailAddress = "b@b.com", BirthDate = new DateOnly(2000, 1, 1) }
            );
            db.SaveChanges();

            var sut = new ContactManager(db);

            var page = sut.GetUserAddressBook(1, pageNumber: 1, pageSize: 2);

            page.Should().HaveCount(2);
            page[0].fullname.Should().Be("Ali Khaled");
            page[1].fullname.Should().Be("Bana Zaki");
        }

        [Fact]
        public void GetContactbyId_Should_Return_Dto_For_Owner()
        {
            using var db = CreateInMemoryContext();
            db.ApplicationUsers.Add(new ApplicationUser { Id = 9, Email = "u@e.com", Password = "x" });
            db.SaveChanges();

            var contact = new Contact
            {
                ApplicationUserId = 9,
                FirstName = "Noor",
                LastName = "Adel",
                PhoneNumber = "999",
                EmailAddress = "noor@example.com",
                BirthDate = new DateOnly(2002, 2, 2)
            };
            db.Contacts.Add(contact);
            db.SaveChanges();

            var sut = new ContactManager(db);
            var dto = sut.GetContactbyId(9, contact.Id);

            dto.Should().NotBeNull();
            dto!.fullname.Should().Be("Noor Adel");
            dto.email.Should().Be("noor@example.com");
            dto.phonenumber.Should().Be("999");
        }
        
        [Fact]
        public void UpdateContact_Should_Update_When_Owned()
        {
            using var db = CreateInMemoryContext();
            db.ApplicationUsers.Add(new ApplicationUser { Id = 42, Email = "owner@example.com", Password = "x" });

            var contact = new Contact
            {
                ApplicationUserId = 42,
                FirstName = "Old",
                LastName = "Name",
                PhoneNumber = "999",
                EmailAddress = "old@example.com",
                BirthDate = new DateOnly(1990, 1, 1)
            };
            db.Contacts.Add(contact);
            db.SaveChanges();

            var sut = new ContactManager(db);

            var dto = new updateContactDto(
                contactId: contact.Id,
                firstname: "New",
                lastname: "Name",
                phoneNumber: "999",             
                email: "new@example.com",
                birthdate: new DateOnly(1995, 5, 5)
            );

            var updated = sut.UpdateContact(dto, userId: 42);

            updated.Should().NotBeNull();
            updated.FirstName.Should().Be("New");
            updated.LastName.Should().Be("Name");
            updated.EmailAddress.Should().Be("new@example.com");
            updated.BirthDate.Should().Be(new DateOnly(1995, 5, 5));

            var reloaded = db.Contacts.Single(c => c.Id == contact.Id && c.ApplicationUserId == 42);
            reloaded.FirstName.Should().Be("New");
            reloaded.EmailAddress.Should().Be("new@example.com");
            reloaded.BirthDate.Should().Be(new DateOnly(1995, 5, 5));
        }

    }
}
