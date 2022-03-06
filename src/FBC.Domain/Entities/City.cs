﻿namespace FBC.Domain.Entities;

public class City : Entity
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public List<User> Users { get; set; } = new();
}