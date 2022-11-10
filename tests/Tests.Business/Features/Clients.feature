@TestCase(TE01)
@Environment(development)
@RestoreDatabase
Feature: Clients

	@TestCode(TE01-001)
	Scenario: Basic Operations
		Given a client with the following data
		  | Field           | Value    |
		  | Identification  | {Random} |
		  | Passport        | {Random} |
		  | First Name      | {Random} |
		  | Last Name       | {Random} |
		  | Gender          | {[M,F]}  |
		  | Primary Phone   | {Random} |
		  | Primary Address | {Random} |
		Then this should be successfully created
		Given the client must be updated using the following data
		  | Field             | Value     |
		  | Id                | {Last:Id} |
		  | Passport          | {Random}  |
		  | Primary Phone     | {Random}  |
		  | Primary Address   | {Random}  |
		  | Secondary Phone   | {Random}  |
		  | Secondary Address | {Random}  |
		Then this should be successfully updated
		Given the client must be deleted using the following data
		  | Field | Value     |
		  | Id    | {Last:Id} |
		Then this should be successfully deleted