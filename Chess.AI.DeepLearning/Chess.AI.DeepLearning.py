
import tensorflow as tf
import numpy as np
import trainingdata

# define learning rate
learning_rate = 0.001

# split data into training and test data
data = np.array(chessdata)
l = int(data.shape[0] * 0.8)
train = data[:l]
test = data[l:]
trainx = train[:, :-1]
trainy = train[:, -1]
testx = test[:, :-1]
testy = test[:, -1]

# create tensorflow placeholders x, y
x = tf.placeholder(tf.float64)
y = tf.placeholder(tf.float64)

# init variables for bias and weights
b = tf.get_variable(name = "b", shape = (1), dtype=tf.float64, initializer=tf.truncated_normal_initializer(stddev=0.1))
w = tf.get_variable(name = "w", shape = (13), dtype=tf.float64, initializer=tf.truncated_normal_initializer(stddev=0.1))

# compute the network prediction of x, w, b
prediction = tf.math.reduce_sum(w * x, axis=1) + b

# compute loss
loss = tf.math.reduce_mean(tf.math.square(prediction - y))

# define a gradient optimizer and optimization operation
optimizer = tf.train.GradientDescentOptimizer(learning_rate)
minimize_op = optimizer.minimize(loss, var_list=[w, b])

# create a new tf session
with tf.Session() as sess:
    
    # init variables
    sess.run(tf.global_variables_initializer())
    
    # run optimization 10000 times
    for i in range(1, 100001):
        
        # minimize loss using optimizer
        sess.run(minimize_op, feed_dict={ x: trainx, y: trainy })
        
        # always calculate the loss after 100 optimizations and print it
        if i % 1000 == 0:
            
            # compute prediction and loss and print it
            train_loss = sess.run(loss, feed_dict={ x: trainx, y: trainy })
            test_loss = sess.run(loss, feed_dict={ x: testx, y: testy })
            print("iteration =", i, "\t train loss =", train_loss, "\t test loss =", test_loss)