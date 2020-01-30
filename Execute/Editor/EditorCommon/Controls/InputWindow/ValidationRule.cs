using System.Windows.Controls;

namespace InputWindow
{
    public interface IInputErrorCheckClass
    {
        ValidationResult IsInputValidate(object value, System.Globalization.CultureInfo cultureInfo);
    }

    public delegate ValidationResult Delegate_ValidateCheck(object value, System.Globalization.CultureInfo cultureInfo);

    public class RequiredRule : ValidationRule
    {
        public Delegate_ValidateCheck OnValidateCheck;

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (OnValidateCheck != null)
                return OnValidateCheck(value, cultureInfo);

            return ValidationResult.ValidResult;
        }
    }
}
