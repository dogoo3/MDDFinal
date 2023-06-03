class Calculator:
  def __init__(self, x, y):
    self.x = x
    self.y = y
  # 더하기 함수
  def add(self):
    return self.x + self.y
 
def main(args):
   t_value = args + 3
   return t_value

def funcname(value):
   return 1+value

if __name__ == "__main__":
    args = 3
    main(args)

# import될 때 호출된다.
print("hello world!!!")
