using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SeriesTracker.Utilities.ValidationRules
{
	public class BadCharsValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			bool success = Regex.Match((value ?? "").ToString(), "[`~!@#$%^&*()_+{}:\"<>?\\-=[\\];',./\\\\| ]").Success;
			return success
				? new ValidationResult(false, "Invalid characters")
				: ValidationResult.ValidResult;
		}
	}
}