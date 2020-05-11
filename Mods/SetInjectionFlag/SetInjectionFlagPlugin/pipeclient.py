f = open(r'\\.\pipe\ModdingTales', 'r+b', 0)
s = "test from python".encode('utf-8')
f.write(s)
f.seek(0)


s = "test 2 from python".encode('utf-8')
f.write(s)
f.seek(0)
# f.close()

# f = open(r'\\.\pipe\ModdingTales', 'r+b', 0)
# print("Server Response: " + f.read().decode("utf-8"))

# s = "test 2 from python".encode('utf-8')
# f.write(s)
# f.seek(0)

# print("Server Response: " + f.read())
