@Browser(Chrome)
@Environment(development)
Feature: Initialization
Initalizing the application

    @TestCase(0)
    Scenario: Initial Scenario
        Given I have a valid configuration
        When I initialize the application
        Then I should get a valid run

    @NotYetImplemented
    Scenario: Not implemented scenario
        Given I have a valid configuration
        When I initialize the application
        Then I should get a valid run