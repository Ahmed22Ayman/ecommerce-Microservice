package com.konecta.order_service.service;

import com.konecta.order_service.model.Order;
import com.konecta.order_service.model.OrderItem;
import com.konecta.order_service.repository.OrderItemRepository;
import com.konecta.order_service.repository.OrderRepository;
import com.konecta.order_service.messaging.OrderCreatedEvent;
import com.konecta.order_service.messaging.OrderEventsConfig;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.amqp.rabbit.core.RabbitTemplate;

import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class OrderService {

    private final OrderRepository orderRepository;
    private final OrderItemRepository orderItemRepository;
    private final RabbitTemplate rabbitTemplate;

    public List<Order> getAll() {
        return orderRepository.findAll();
    }

    public Order getById(Long id) {
        return orderRepository.findById(id).orElseThrow(() -> new RuntimeException("Order not found"));
    }

    @Transactional
    public Order create(Order order) {
        // wire back-references for cascade
        if (order.getItems() != null) {
            for (OrderItem item : order.getItems()) {
                item.setOrder(order);
            }
        }
        Order saved = orderRepository.save(order);

        // Publish OrderCreated event
        OrderCreatedEvent event = new OrderCreatedEvent(
                saved.getOrderId(),
                saved.getUserId(),
                saved.getTotalAmount(),
                saved.getItems() == null ? List.of() : saved.getItems().stream()
                        .map(i -> new OrderCreatedEvent.Item(i.getProductId(), i.getQuantity(), i.getPrice()))
                        .collect(Collectors.toList())
        );
        rabbitTemplate.convertAndSend(OrderEventsConfig.EXCHANGE, OrderEventsConfig.ORDER_CREATED_ROUTING_KEY, event);

        return saved;
    }

    @Transactional
    public Order update(Long id, Order updated) {
        Order existing = getById(id);
        existing.setStatus(updated.getStatus());
        existing.setTotalAmount(updated.getTotalAmount());
        existing.getItems().clear();
        if (updated.getItems() != null) {
            for (OrderItem item : updated.getItems()) {
                item.setOrder(existing);
                existing.getItems().add(item);
            }
        }
        return orderRepository.save(existing);
    }

    public void delete(Long id) {
        orderRepository.deleteById(id);
    }
}
