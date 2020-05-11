import socket

port = 999
msg = 'Message to TaleSpire\n'.encode('utf-8')

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect(('127.0.0.1', port))
s.send(msg)
print(s.recv(2048))
s.close()