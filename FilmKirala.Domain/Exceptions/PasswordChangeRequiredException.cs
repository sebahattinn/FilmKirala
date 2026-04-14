namespace FilmKirala.Domain.Exceptions
{
    public class PasswordChangeRequiredException : Exception
    {
        public string Code => "PASSWORD_CHANGE_REQUIRED";

        public PasswordChangeRequiredException()
            : base("Your password has expired. Please change your password to continue.") { }
    }
}
