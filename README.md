# EconGame
An economics inspired personal game dev project.

Market behavior:
No generic predetermined prices for goods.
Goods are transferred in the market through transactions.
A transaction happens between two economic agents. One is the buyer and the other is the seller.
The two have their own prices. If the seller's price is lower than the buyer's asking price, resources exchange hands.
Transaction matching works in a way that maximizes sales: cheaper goods are bought by the poorest.
When not enough goods are available, the poorest bidder gets the good since richer people can probably buy something else.
[temp] Prioritizing poor/rich might be changed to the opposite or into an option after testing.

Economic Agents behavior:
Economic agents decide on which goods they want more, and how much they want that for.
Each good provides the agent with utility such as health, happiness, or ingredients for production.
The agents calculate how much it is worth by (total util provided) * (price per util)
Both elements change as the game progresses based on law of scarcity.

Production:
Production is done in buildings.
Buildings hire workers and buy ingredients from other buildings to produce goods. These make up the total cost.
Base amount produced by a worker is predetermined. This can be increased by efficiency: skilled workers and better gear.
The selling price of the produced good is determined by (total cost) / (number produced) + premium.
Premium is increaed and decreased based on sales. If sales are good, increase. If not, decrease til 0.
If premium is 0 and still not good sales, decrease wage.
[temp] Asking price for ingredients are based on past average sales.

Population:
People are the economic agents that generate labor and consumption.
They purchase goods from the market to gain health and happiness.
How much utility a good provides depends on the peorson's current standing: Better off people want different stuff than worse off people.
Each person has different strength, intelligence, and personability; making some apt more for certain jobs and less for others.
Their talents develop as they gain experience at a workplace.
More skilled workers get hired first. Workers go to jobs that pay higher wages first.
The healthier and happier a person is, the more they will contribute to population growth.
[temp] Population doesn't decrease due to insufficient goods supply.
When population growth reaches a certain threshhold, a new person is added to the population.
The threshhold grows as population grows.
[temp] They start off with 0 money and lowest possible skill and 50 health and 50 happiness.
