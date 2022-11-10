@TestCase(TE01)
@Environment(development)
@RestoreDatabase
Feature: Clients

	@TestCode(TE01-001)
	Scenario: Basic Operations
		Given a client with the following data
		  | Field          | Value        |
		  | Identification | {Random}     |
		  | Full Name      | {Random}     |
		  | Gender         | {M,F}        |
		  | Address        | {Random:100} |
		  | Phone Number   | {Random:7}   |
		  | Category       | {Random:10}  |
		Then this should be successfully created
		Given the client must be partially updated using the following data
		  | Field   | Value        |
		  | Id      | {Last:Id}    |
		  | Gender  | {M,F}        |
		  | Address | {Random:100} |
		Then this should be successfully updated
		Given the client must be deleted using the following data
		  | Field | Value     |
		  | Id    | {Last:Id} |
		Then this should be successfully deleted