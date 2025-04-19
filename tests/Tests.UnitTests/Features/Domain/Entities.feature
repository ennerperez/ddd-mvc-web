Feature: Entities

    Background:
        Given the Domain.Entities namespace in the Domain assembly

    @TestCase(0)
    Scenario: Budget
        Given the following data
          | Field        | Value      |
          | Code         | {Random}   |
          | Id           | {Guid}     |
          | Status       | 1          |
          | ClientId     | 1          |
          | Client       | {Instance} |
          | Subtotal     | 1.0        |
          | Total        | 1.0        |
          | Taxes        | 1.0        |
          | ExpireAt     | {2dAhead}  |
          | CreatedAt    | {2dAgo}    |
          | CreatedById  | 1          |
          | ModifiedAt   | {1dAgo}    |
          | ModifiedById | 1          |
          | DeletedById  | 1          |
          | IsDeleted    | true       |
          | DeletedAt    | {Now}      |
        Then a Budget should be created
        And all Budget ctors should be used

    @TestCase(1)
    Scenario: Client
        Given the following data
          | Field          | Value    |
          | Code           | {Random} |
          | Identification | {Random} |
          | FullName       | {Random} |
          | Address        | {Random} |
          | PhoneNumber    | {Random} |
          | Category       | {Random} |
          | Id             | 1        |
          | CreatedAt      | {2dAgo}  |
          | ModifiedAt     | {1dAgo}  |
        Then a Client should be created
        And all Client ctors should be used

    @TestCase(2)
    Scenario: Setting
        Given the following data
          | Field      | Value    |
          | Id         | 1        |
          | Key        | {Random} |
          | Type       | 7        |
          | Value      | {Random} |
          | CreatedAt  | {2dAgo}  |
          | ModifiedAt | {1dAgo}  |
        Then a Setting should be created
        And all Setting ctors should be used

    @TestCase(3)
    Scenario: Country
        Given the Domain.Entities.Cache namespace in the Domain assembly
        Given the following data
          | Field   | Value    |
          | Name    | {Random} |
          | ISO3166 | {Random} |
          | Id      | 1        |
        Then a Country should be created
        And all Country ctors should be used