using Contact_Management_system.Dtos;
using Contact_Management_system.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Contact_Management_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly IContactManager _contactManager;
        private readonly Validations _validations;
        

        public ContactsController(IContactManager contactManager, Validations validations)
        {
            _contactManager = contactManager;
            _validations = validations;
        }

        [HttpPost]
        public ActionResult<addContactResponseDto> Add([FromBody] addContactDto data)
        {
            if (!ModelState.IsValid)
                return BadRequest(new addContactResponseDto(false, "Invalid payload."));

            if (!_validations.ValidatePhoneNumber(data.phonenumber)) return BadRequest("Phone Number should be all numbers");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = _contactManager.AddContact(data, int.Parse(userId!));

            if (!result.success)
                return BadRequest(result);

            
            return Created();
        }

        [HttpGet("GetAllContacts")]
        public ActionResult<IReadOnlyList<readContactDto>> GetAddressBook(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = _contactManager.GetUserAddressBook(int.Parse(userid!), pageNumber, pageSize);
            return Ok(data);
        }

        [HttpGet("{contactId:int}")]
        public ActionResult<readContactDto> GetContact([FromRoute] int contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var contact = _contactManager.GetContactbyId(int.Parse(userId!), contactId);

            if (contact is null)
                return NotFound("Contact not found for this user.");

            return Ok(contact);
        }

        [HttpPut]
        public ActionResult Update([FromBody] updateContactDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid payload.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var updated = _contactManager.UpdateContact(dto, int.Parse(userId!));
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Contact not found for this user/phone.");
            }
        }

        [HttpDelete("{contactId:int}")]
        public IActionResult Delete(int contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                _contactManager.DeleteContact(int.Parse(userId!), contactId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Contact not found for this user.");
            }
        }
    }
}
