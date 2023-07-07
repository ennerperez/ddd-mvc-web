@Browser(Chrome)
@Environment(development)
Feature: Account

	@TestCase(0)
	Scenario: Login
		Given the user is at the "Main" page
		When the user clicks the "Login" button