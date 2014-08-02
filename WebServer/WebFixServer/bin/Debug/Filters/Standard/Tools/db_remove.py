import sqlite3
conn = sqlite3.connect("dictionary.db")
c = conn.cursor()

while True:
	c = conn.cursor()
	c.execute("""delete from dictionary where key=?""",(input("What to delete?"),))

	conn.commit()
	c.close()