Feature: Mock tests

Background:
	Given I use the mock
	

Scenario: Make normal payment
	Given I have a transaction with Debit for value 100
	When the transaction is processed
	Then the exception will be empty
		And the result will contain
			| Text                         |
			| Ask Table Syncronization     |
			| Table Updated: True          |
			| Synchronize Tables called    |
			| Transaction Status: Accepted |
			| Payment Processed: Accepted  |
		And the result will not contain
			| Text           |
			| I GOT ERROR 11 |

Scenario: Problem with initializing
	Given I have a transaction with Debit for value 100
		But there is a problem on initialization
	When the transaction is processed
	Then the exception will not be empty
		And the result will not contain
			| Text                         |
			| Ask Table Syncronization     |
			| Table Updated: True          |
			| Synchronize Tables called.   |
			| Transaction Status: Accepted |
			| Payment Processed: Accepted  |

Scenario: Problem with updating tables
	Given I have a transaction with Debit for value 100
		But there is a problem on updating tables
	When the transaction is processed
	Then the exception will not be empty
		And the result will not contain
			| Text                         |
			| Ask Table Syncronization     |
			| Table Updated: True          |
			| Synchronize Tables called.   |
			| Transaction Status: Accepted |
			| Payment Processed: Accepted  |

