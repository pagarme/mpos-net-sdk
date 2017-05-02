Feature: Machine Tests

@need_device
Scenario: Make payment with the Machine
	Given I use the machine
		And I have a transaction with Debit for value 100
	When the transaction is processed
	Then the exception will be empty
		And the result will contain
			| Text                         |
			| Ask Table Syncronization     |
			| Table Updated: True          |
			| Synchronize Tables called    |
			| Transaction Status: Accepted |
			| Payment Processed: Accepted  |
		#And the result will not contain
		#	| Text        |
		#	| Errored: 11 |
