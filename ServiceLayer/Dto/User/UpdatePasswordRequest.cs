namespace ServiceLayer.Dto.User
{
    public class UpdatePasswordRequest
    {
        public string NewPassword { get; set; }
        public string CurrentPassword { get; set; }

        public UpdatePasswordRequest() { }
        public UpdatePasswordRequest(string newPassword, string currentPassword)
        {
            NewPassword = newPassword;
            CurrentPassword = currentPassword;
        }
    }
}
