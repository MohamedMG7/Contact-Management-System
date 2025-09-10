using Contact_Management_system.DbHelper;
using Contact_Management_system.Dtos;
using Contact_Management_system.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Contact_Management_system.Managers
{
    public interface IContactManager {
        addContactResponseDto AddContact(addContactDto data, int userId);
        IReadOnlyList<readContactDto> GetUserAddressBook(int userId, int pageNumber, int pageSize);
        Contact UpdateContact(updateContactDto data, int userId);
        void DeleteContact(int userId, int contactId);

        readContactDto GetContactbyId(int userId, int contactId);
    }
    public class ContactManager : IContactManager
    {
        private readonly ApplicationDbContext _context;
       

        public ContactManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public addContactResponseDto AddContact(addContactDto data, int userId) {
            // basic validation for data
            if (data is null
                || userId <= 0
                || string.IsNullOrWhiteSpace(data.firstname)
                || string.IsNullOrWhiteSpace(data.lastname))
            {
                return new addContactResponseDto(false, message: "data entered not correct.");
            }

            // check if the contact already exist
            var userExists = _context.Set<ApplicationUser>().Any(u => u.Id == userId);
            if (!userExists)
                return new addContactResponseDto(false, message:"User not found.");


            // check if the user already has this contact in his book
            bool duplicate = _context.Set<Contact>().Where(c => c.ApplicationUserId == userId)
        .   Any(c =>(!string.IsNullOrEmpty(data.phonenumber) && c.PhoneNumber == data.phonenumber)
            );

            if (duplicate)
                return new addContactResponseDto(false, message:"Contact with same phone already exists for this user.");

            var entity = new Contact
            {
                ApplicationUserId = userId,
                FirstName = data.firstname.Trim(),
                LastName = data.lastname.Trim(),
                PhoneNumber = data.phonenumber,
                EmailAddress = data.email,
                BirthDate = data.birthdate
            };

            _context.Add(entity);
            var saved = _context.SaveChanges() > 0;

            return saved ? new addContactResponseDto(true, message:"Contact added."): new addContactResponseDto(false, message:"Failed to save contact.");
        }

        public IReadOnlyList<readContactDto> GetUserAddressBook(int userId, int pageNumber, int pageSize) {
            // set limits to the pagination
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 20);

            var skip = (pageNumber - 1) * pageSize;

            var query = _context.Set<Contact>()
            .AsNoTracking()
            .Where(c => c.ApplicationUserId == userId)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.BirthDate);

            var page = query
                .Skip(skip)
                .Take(pageSize)
                .Select(c => new readContactDto(
                    c.FullName,
                    c.PhoneNumber,
                    c.EmailAddress,
                    c.BirthDate
                ))
                .ToList();

            return page;
        }
        public readContactDto GetContactbyId(int userId, int contactId)
        {
            if (contactId <= 0 || userId <= 0) throw new ArgumentException("Invalid Input");

            var contact = _context.Set<Contact>().AsNoTracking().Where(c => c.Id == contactId && c.ApplicationUserId == userId)
                .Select( c=> new readContactDto(
                c.FullName,
                c.PhoneNumber,
                c.EmailAddress,
                c.BirthDate
            )).FirstOrDefault();

            return contact;
        }

        public Contact UpdateContact(updateContactDto data, int userId)
        {
            if (data is null || userId <= 0 || string.IsNullOrWhiteSpace(data.phoneNumber))
                throw new ArgumentException("Invalid input");

            var contact = _context.Set<Contact>()
                .FirstOrDefault(c => c.ApplicationUserId == userId && c.Id == data.contactId);

            if (contact is null)
                throw new KeyNotFoundException("Contact not found for this user/phone.");

            contact.FirstName = data.firstname ?? string.Empty;
            contact.LastName = data.lastname ?? string.Empty;
            contact.EmailAddress = data.email ?? string.Empty;
            contact.BirthDate = data.birthdate;

            _context.Update(contact);
            _context.SaveChanges();

            return contact;
        }


        public void DeleteContact(int userId, int contactId)
        {
            if (userId <= 0 || contactId <= 0)
                throw new ArgumentException("Invalid userId or contactId.");
            
            // make sure the user has this contact
            var contact = _context.Set<Contact>()
                .FirstOrDefault(c => c.Id == contactId && c.ApplicationUserId == userId);

            if (contact is null)
                throw new KeyNotFoundException("Contact not found for this user.");

            _context.Remove(contact);
            _context.SaveChanges();
        }
    }
}
