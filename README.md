# EconGame
An economics inspired personal game dev project.

Market behavior:
No generic predetermined prices for goods.
Goods are transferred in the market through transactions.
A transaction happens between two economic agents. One is the buyer and the other is the seller.
The two have their own prices. If the seller's price is lower than the buyer's asking price, resources exchange hands.

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
