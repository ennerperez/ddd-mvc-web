@Browser(Chrome)
@Environment(development)
Feature: Account

	@TestCase(0)
	Scenario: Login
		Given I am at the "Main" page
		Then I click on the "Login" button