namespace Contact_Management_system.Dtos
{
    public record addContactDto(string firstname, string lastname, string phonenumber, DateOnly birthdate,string email);
    public record readContactDto(string fullname, string phonenumber, string email, DateOnly birthdate);
    public record addContactResponseDto(bool success, string? message);
    public record updateContactDto(int contactId, string firstname, string lastname, string phoneNumber, string email, DateOnly birthdate);

}
