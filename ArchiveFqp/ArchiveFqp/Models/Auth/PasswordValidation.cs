using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Auth
{
    public class PasswordValidation : ValidationAttribute
    {
        private readonly int _minLength;
        private readonly bool _requireDigit;
        private readonly bool _requireLowercase;
        private readonly bool _requireUppercase;
        private readonly bool _requireNonAlphanumeric;

        public PasswordValidation(int minLength, bool requireDigit, bool requireLowercase, bool requireUppercase, bool requireNonAlphanumeric)
        {
            _minLength = minLength;
            _requireDigit = requireDigit;
            _requireLowercase = requireLowercase;
            _requireUppercase = requireUppercase;
            _requireNonAlphanumeric = requireNonAlphanumeric;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string password = value as string ?? "";
            string? errMessage = null;

            if (string.IsNullOrWhiteSpace(password))
                errMessage = "Пароль не может быть пустым";

            if (password.Trim() != password)
                errMessage = "Пароль должен быть без пробелов";

            if (password.Length < _minLength)
                errMessage = $"Пароль должен иметь больше {_minLength} символов";

            if (_requireDigit && !password.Any(char.IsDigit))
                errMessage = "Пароль должен иметь хотя бы одну цифру";

            if (_requireLowercase && !password.Any(char.IsLower))
                errMessage = "Пароль должен иметь хотя бы одну прописную букву";

            if (_requireUppercase && !password.Any(char.IsUpper))
                errMessage = "Пароль должен иметь хотя бы одну заглавную букву";

            if (_requireNonAlphanumeric && !password.Any(c => !char.IsLetterOrDigit(c)))
                errMessage = "Пароль должен иметь хотя бы один сторонний символ (не алфавита и не цифры)";

            if (errMessage != null)
                return new ValidationResult(errMessage, [validationContext.MemberName!]);

            return ValidationResult.Success;
        }
    }
}
