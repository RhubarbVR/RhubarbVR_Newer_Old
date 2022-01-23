namespace SharedModels
{
	public class AccountCreationErrorResponse : AccountCreationResponse
	{
		public AccountCreationErrorResponse() {
			Error = true;
		}

		public AccountCreationErrorResponse(string message) {
			Message = message;
			Error = true;
		}

		public static AccountCreationResponse GenericRegistrationError() {
			return new AccountCreationResponse() {
				Message = "There was an error Creating your Account",
				ErrorDetails = "Email might have already been used or your password does not meet our requirements or Username has been used to many times"
			};
		}
	}
}