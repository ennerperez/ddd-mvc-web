Feature: Services

    @TestCase(0)
    Scenario: Document Service
        Given the service compose a new test document
        Then the test document is generated as pdf
        Then the test document is generated as xlsx
        Then the test document is generated as csv
        
    @TestCase(1)
    Scenario: File Service
        Given the service create a new test file
        Then the test file should be created
        
    @TestCase(2)
    Scenario: Email Service
        Given the service send an email to noreply@test.com
        Then the email should be received
