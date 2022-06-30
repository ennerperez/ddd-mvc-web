@Browser(Chrome)
@Environment(development)
Feature: Account

    @TestCase(0)
    Scenario: Login
        Given I am at the "Main" page
        And I click on "Login" button